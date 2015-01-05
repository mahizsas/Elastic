using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public enum SUBSCRIPTION_TYPE
    {
        FREE = 0,
        PREMIUM
    }
    public class Subscriptions : EntityData
    {
        public Subscriptions()
        {
            StartingDate = DateTime.Now;
            ExpirationDate = DateTime.Now;
        }
        public string CompaniesID { get; set; }
        public Companies Company { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; }
        public string Token { get; set; }
        public string AccessToken { get; set; }
        public string RedirectUrl { get; set; }
        public SUBSCRIPTION_TYPE Type { get; set; }
    }
}