using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DTO
{
    public class MobileGroup : EntityData
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public bool Visible { get; set; }
    }
}