using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class Contacts : EntityData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string ImageUrl { get; set; }

        public bool Visible { get; set; }

        public virtual string GroupsID { get; set; }

        internal virtual Groups Groups { get; set; }

        internal virtual ICollection<Favorites> Favorites { get; set; }


    }
}