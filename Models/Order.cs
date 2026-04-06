using System.Text.Json.Serialization;
namespace OrdersDownloader.Models
{
    public class Order
    {
        public long id { get; set; }
        public string state { get; set; }
        public DateTime date_created { get; set; }

        public string client_first_name { get; set; }
        public string client_last_name { get; set; }
        public string client_second_name { get; set; }

        public Client client { get; set; }

        public string email { get; set; }
        public string phone { get; set; }

        public string delivery_address { get; set; }
        public DeliveryOption delivery_option { get; set; }
        public PaymentOption payment_option { get; set; }
        public decimal? delivery_cost { get; set; }
        public decimal total_price { get; set; }

        public string client_comment { get; set; }
        public string seller_comment { get; set; }

        public List<OrderItem> products { get; set; }

    }
}