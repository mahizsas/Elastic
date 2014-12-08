using Microsoft.WindowsAzure.Mobile.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mpbdmService.DataObjects
{
    public class Companies : EntityData
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        internal virtual ICollection<Users> Users { get; set; }

        internal virtual ICollection<Groups> Groups { get; set; }

    }
}