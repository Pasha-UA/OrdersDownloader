using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersDownloader.Models
{
    public class Config
    {
        public PromApiConfig PromApi { get; set; }
    }

    public class PromApiConfig
    {
        public string BaseUrl { get; set; }
        public string Token { get; set; }
        public string OrdersListApiUrl { get; set; }
    }
}