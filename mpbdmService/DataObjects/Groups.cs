using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class Groups: EntityData
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public bool Visible { get; set; }

        internal virtual ICollection<Contacts> Contacts { get; set; }

        public virtual string CompaniesID { get; set; }

        internal virtual Companies Companies { get; set; }

    }
    
}