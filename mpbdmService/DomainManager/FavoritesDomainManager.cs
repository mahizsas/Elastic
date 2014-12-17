using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DataObjects;
using mpbdmService.DTO;
using mpbdmService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;

namespace mpbdmService.DomainManager
{
    public class FavoritesDomainManager : MappedEntityDomainManager<MobileFavorites, Favorites>
    {
        public IPrincipal User;
        private EntityDomainManager<Favorites> domainManager;
        public FavoritesDomainManager(mpbdmContext context, HttpRequestMessage request, ApiServices services, IPrincipal User)
            : base(context, request, services , true)
        {
            this.User = User;
            domainManager = new EntityDomainManager<Favorites>(context, Request, Services, true);
        }


        public override IQueryable<MobileFavorites> Query()
        {
            //var currentId = "Google:105535740556221909032";
            var currentUser = User as ServiceUser;
            var currentId = currentUser.Id;
            // REMEMBER TO USE Where(s=>s.Deleted==false) CAUSE I M not using base.QUERY()
            IQueryable<MobileFavorites> favs = domainManager.Query().Where(s=>s.Deleted == false).Where(s => s.UsersID == currentId ).Select(Mapper.Map<MobileFavorites>).AsQueryable();
            return favs;
        }

        public override System.Threading.Tasks.Task<bool> DeleteAsync(string id)
        {
            return base.DeleteItemAsync(id);
        }

        public override SingleResult<MobileFavorites> Lookup(string id)
        {
            return base.LookupEntity(s => s.Id == id);
        }

        public async override System.Threading.Tasks.Task<MobileFavorites> UpdateAsync(string id, Delta<MobileFavorites> patch)
        {

            IEnumerable<string> names = patch.GetChangedPropertyNames();
            var np = new Delta<Favorites>();
            foreach (string name in names)
            {
                object obj;
                patch.TryGetPropertyValue(name, out obj);
                np.TrySetPropertyValue(name, obj);
            }
            await domainManager.UpdateAsync(id, np);


            Favorites data = await this.Context.Set<Favorites>().FindAsync(id);
            return Mapper.Map<MobileFavorites>(data);
        }
        public async override System.Threading.Tasks.Task<MobileFavorites> InsertAsync(MobileFavorites data)
        {
            Favorites newData = new Favorites();

            Mapper.Map<MobileFavorites, Favorites>(data, newData);

            var user = User as ServiceUser;
            if (data.Id == null)
            {
                newData.Id = Guid.NewGuid().ToString();
            }
            else
            {
                newData.Id = data.Id;
            }
            newData.UsersID = user.Id;

            await domainManager.InsertAsync(newData);

            return Mapper.Map<MobileFavorites>(newData);
        }
    }
}