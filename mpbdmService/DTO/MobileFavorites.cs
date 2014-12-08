using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class MobileFavorites : EntityData
    {
        public bool Visible { get; set; }
        public virtual string ContactsID { get; set; }
    }
}