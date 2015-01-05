using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class Transactions : EntityData
    {
        public string Sender { get; set; }
        public string Reciever { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }
        public double SenderMoney { get; set; }
        public double RecieverMoney { get; set; }
        public string PaymentId { get; set; }
        public string Token { get; set; }
        public string AccessToken { get; set; }
        public string RedirectUrl { get; set; }
        
    }
}