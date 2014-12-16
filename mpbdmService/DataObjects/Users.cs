using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class Users : EntityData
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string ImageUrl { get; set; }

        public virtual string CompaniesID { get; set; }

        public virtual Companies Companies { get; set; }

        internal virtual ICollection<Favorites> Favorites { get; set; }


    }
}