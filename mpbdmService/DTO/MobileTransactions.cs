using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DTO
{
    public class MobileTransactions : EntityData
    {
        public string Sender { get; set; }
        public string Reciever { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }
        public double Money { get; set; }
    }
}