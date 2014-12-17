using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DataObjects;
using mpbdmService.DTO;
using mpbdmService.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;

namespace mpbdmService.DomainManager
{
    public class GroupsDomainManager : MappedEntityDomainManager<MobileGroup, Groups>
    {
        public IPrincipal User;
        private EntityDomainManager<Groups> domainManager;
        
        public GroupsDomainManager( mpbdmContext context , HttpRequestMessage request , ApiServices services ) 
                    : base ( context , request , services , true )
        {
            domainManager = new EntityDomainManager<Groups>(context, Request, Services, true);
        }

        public override IQueryable<MobileGroup> Query()
        {
            //var currentId = "Google:105535740556221909032";
            var currentUser = User as ServiceUser;
            var currentId = currentUser.Id;
            var cid = GetCompanyId(currentId);
            // REMEMBER TO WHERE ( s => s.Deleted == false ) BECAUSE I DONT USE THE BASE.QUERY()
            IQueryable<MobileGroup> groups = domainManager.Query().Where(s=>s.Deleted == false).Where(s => s.Companies.Id == cid).Select(Mapper.Map<MobileGroup>).AsQueryable();
            return groups;
        }

        private string GetCompanyId(string currentId)
        {
            var user = this.Context.Set<Users>().Where(s => s.Id == currentId).FirstOrDefault();
            if( user == null ) return null;
            return user.CompaniesID; 
        }


        public override System.Threading.Tasks.Task<bool> DeleteAsync(string id)
        {
            return base.DeleteItemAsync(id);
        }

        public override System.Web.Http.SingleResult<MobileGroup> Lookup(string id)
        {
            return base.LookupEntity(s => s.Id == id);
        }

        public override async System.Threading.Tasks.Task<MobileGroup> UpdateAsync(string id, Delta<MobileGroup> patch)
        {
            Groups data = await this.Context.Set<Groups>().FindAsync(id);

            MobileGroup mobgroup = Mapper.Map<Groups , MobileGroup>(data);

            IEnumerable<string> names = patch.GetChangedPropertyNames();
            var np = new Delta<Groups>( );
            foreach( string name in names ){
                object obj;
                patch.TryGetPropertyValue(name,out obj);
                np.TrySetPropertyValue(name , obj);
            }
            await domainManager.UpdateAsync(id , np);
            
            return Mapper.Map<MobileGroup>(data);
        }

        public override async System.Threading.Tasks.Task<MobileGroup> InsertAsync(MobileGroup data)
        {
            Groups newData = new Groups();

            Mapper.Map<MobileGroup, Groups>(data, newData);

            var user = User as ServiceUser;
            if (data.Id == null)
            {
                newData.Id = Guid.NewGuid().ToString();
            }
            else
            {
                newData.Id = data.Id;
            }
            newData.CompaniesID = GetCompanyId(user.Id);
            newData.Visible = true;

            await domainManager.InsertAsync(newData);

            return Mapper.Map<MobileGroup>(newData);
        }
    }
}