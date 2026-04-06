using OrdersDownloader.Models;
using System.Globalization;
using System.Xml.Linq;

namespace OrdersDownloader.Services
{
    public class OrderExporter
    {
        public static XDocument ExportOrderToCml(Order order)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("КоммерческаяИнформация",
                    new XAttribute("ВерсияСхемы", "2.05"),
                    new XAttribute("ФорматДаты", "ДФ=yyyy-MM-dd; ДЛФ=DT"),
                    new XAttribute("ФорматВремени", "ДФ=ЧЧ:мм:сс; ДЛФ=T"),
                    new XAttribute("РазделительДатаВремя", "T"),
                    new XAttribute("ФорматСуммы", "ЧЦ=18; ЧДЦ=2; ЧРД=."),
                    new XAttribute("ФорматКоличества", "ЧЦ=18; ЧДЦ=2; ЧРД=."),
                    new XAttribute("ДатаФормирования", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                    new XElement("Документ",
                        new XElement("Ид", order.id),
                        new XElement("Номер", order.id),
                        new XElement("Дата", order.date_created.ToString("yyyy-MM-dd")),
                        new XElement("Время", order.date_created.ToString("HH:mm:ss")),
                        new XElement("ХозОперация", "Заказ товара"),
                        new XElement("Роль", "Продавец"),
                        new XElement("СкидкаПоПромокоду", string.Empty),
                        new XElement("ПримечаниеКлиента", order.client_comment ?? string.Empty),
                        new XElement("ЗначенияРеквизитов",
                            new XElement("ЗначениеРеквизита",
                                new XElement("Наименование", "Способ доставки"),
                                new XElement("Значение", order.delivery_option?.name ?? "")
                            ),
                            new XElement("ЗначениеРеквизита",
                                new XElement("Наименование", "Адрес доставки"),
                                new XElement("Значение", order.delivery_address ?? "")
                            ),
                            new XElement("ЗначениеРеквизита",
                                new XElement("Наименование", "Статус заказа"),
                                new XElement("Значение", order.state ?? "")
                            )
                        ),
                        new XElement("Контрагенты",
                            new XElement("Контрагент",
                                new XElement("Ид", order.client != null ? order.client.phone : "0"),
                                new XElement("Наименование", $"{order.client_first_name} {order.client_last_name}"),
                                new XElement("ПолноеНаименование", $"{order.client_first_name} {order.client_last_name}"),
                                new XElement("Фамилия", order.client_last_name ?? ""),
                                new XElement("Имя", order.client_first_name ?? ""),
                                new XElement("Контакты",
                                    new XElement("Контакт",
                                        new XElement("Тип", "Почта"),
                                        new XElement("Значение", order.email ?? "")
                                    ),
                                    new XElement("Контакт",
                                        new XElement("Тип", "Телефон"),
                                        new XElement("Значение", order.phone ?? "")
                                    ),
                                    new XElement("Контакт",
                                        new XElement("Тип", "ТелефонРабочий"),
                                        new XElement("Значение", order.phone ?? "")
                                    )
                                )
                            )
                        ),
                        new XElement("Товары",
                            from p in order.products
                            select new XElement("Товар",
                                new XElement("Ид", p.id),
                                new XElement("ИдКаталога", p.external_id ?? ""),
                                new XElement("Наименование", p.name ?? ""),
                                new XElement("БазоваяЕдиница", p.measure_unit ?? "шт."),
                                new XElement("ЦенаЗаЕдиницу", p.price?.ToString("F2", CultureInfo.InvariantCulture) ?? "0.00"),
                                new XElement("Валюта", "UAH"),
                                new XElement("Количество", ((decimal)p.quantity).ToString("F2", CultureInfo.InvariantCulture)),
                                new XElement("Сумма", ((p.price ?? 0) * p.quantity).ToString("F4", CultureInfo.InvariantCulture)),
                                new XElement("ЗначенияРеквизитов",
                                    new XElement("ЗначениеРеквизита",
                                        new XElement("Наименование", "ВидНоменклатуры"),
                                        new XElement("Значение", "Товар")
                                    ),
                                    new XElement("ЗначениеРеквизита",
                                        new XElement("Наименование", "ТипНоменклатуры"),
                                        new XElement("Значение", "Товар")
                                    )
                                )
                            )
                        )
                    )
                )
            );

            return doc;
        }

        public void ExportToCml(List<Order> orders, string fileName)
        {
            // Создаём корень CML для всех заказов
            var root = new XElement("КоммерческаяИнформация",
                new XAttribute("ВерсияСхемы", "2.05"),
                new XAttribute("ФорматДаты", "ДФ=yyyy-MM-dd; ДЛФ=DT"),
                new XAttribute("ФорматВремени", "ДФ=ЧЧ:мм:сс; ДЛФ=T"),
                new XAttribute("РазделительДатаВремя", "T"),
                new XAttribute("ФорматСуммы", "ЧЦ=18; ЧДЦ=2; ЧРД=."),
                new XAttribute("ФорматКоличества", "ЧЦ=18; ЧДЦ=2; ЧРД=."),
                new XAttribute("ДатаФормирования", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"))
            );

            foreach (var order in orders)
            {
                // Берём отдельный XDocument для заказа
                XDocument orderDoc = ExportOrderToCml(order);

                // Берём только <Документ> и добавляем его в общий корень
                if (orderDoc.Root != null)
                {
                    var docElement = orderDoc.Root.Element("Документ");
                    if (docElement != null)
                    {
                        root.Add(docElement);
                    }
                }
            }

            // Сохраняем финальный XML
            var finalDoc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);

            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "1C_Exchange"
            );

            // создаём папку если нет
            Directory.CreateDirectory(downloadsPath);

            // полный путь к файлу
            var fullPath = Path.Combine(downloadsPath, fileName);


            finalDoc.Save(fullPath);

            Console.WriteLine($"CML saved: {fullPath}");
        }
    }
}