using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class Favorites : EntityData
    {
        public bool Visible { get; set; }

        public virtual string UsersID { get; set; }

        internal virtual Users Users { get; set; }

        public virtual string ContactsID { get; set; }

        internal virtual Contacts Contacts { get; set; }
    }
}