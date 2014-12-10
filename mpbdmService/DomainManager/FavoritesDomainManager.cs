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

namespace mpbdmService.DomainManager
{
    public class FavoritesDomainManager : MappedEntityDomainManager<MobileFavorites, Favorites>
    {
        public IPrincipal User;
        public FavoritesDomainManager(mpbdmContext<Guid> context, HttpRequestMessage request, ApiServices services, IPrincipal User)
            : base(context, request, services , true)
        {
            this.User = User;
            
        }


        public override IQueryable<MobileFavorites> Query()
        {
            //var currentId = "Google:105535740556221909032";
            var currentUser = User as ServiceUser;
            var currentId = currentUser.Id;
            // REMEMBER TO USE Where(s=>s.Deleted==false) CAUSE I M not using base.QUERY()
            IQueryable<MobileFavorites> favs = this.Context.Set<Favorites>().Where(s=>s.Deleted == false).Where(s => s.UsersID == currentId ).Select(Mapper.Map<MobileFavorites>).AsQueryable();
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

        public async override System.Threading.Tasks.Task<MobileFavorites> UpdateAsync(string id, System.Web.Http.OData.Delta<MobileFavorites> patch)
        {
            Favorites data = await this.Context.Set<Favorites>().FindAsync(id);

            MobileFavorites mobfav = Mapper.Map<Favorites , MobileFavorites>(data);
            
            var a = patch.GetChangedPropertyNames();

            patch.Patch(mobfav);

            Mapper.Map<MobileFavorites, Favorites>(mobfav, data);
            
            await this.Context.SaveChangesAsync();

            return Mapper.Map<MobileFavorites>(data);
        }
        public async override System.Threading.Tasks.Task<MobileFavorites> InsertAsync(MobileFavorites data)
        {
            Favorites newData = new Favorites();

            Mapper.Map<MobileFavorites, Favorites>(data, newData);

            var user = User as ServiceUser;
            newData.Id = Guid.NewGuid().ToString();
            newData.UsersID = user.Id;

            this.Context.Set<Favorites>().Add(newData);

            await this.Context.SaveChangesAsync();

            return Mapper.Map<MobileFavorites>(newData);
        }
    }
}