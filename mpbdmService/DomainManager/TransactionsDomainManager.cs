using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.DataObjects;
using mpbdmService.DTO;
using mpbdmService.ElasticScale;
using mpbdmService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;

namespace mpbdmService.DomainManager
{
    // TODO: Need to fix architect 
    //       Need to Clear code 
    //       Need to move data management to Entity Domain Manager

    public class TransactionsDomainManager : MappedEntityDomainManager<MobileTransactions, Transactions>
    {
        public IPrincipal User;
        private EntityDomainManager<Transactions> domainManager;
        public TransactionsDomainManager(mpbdmContext<Guid> context, HttpRequestMessage request, ApiServices services)
            : base(context, request, services , true)
        {
            domainManager = new EntityDomainManager<Transactions>(context, request, services, true);
        }


        public override IQueryable<MobileTransactions> Query()
        {

            string shardKey = Sharding.FindShard(User);
            domainManager.Context = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);
            // REMEMBER TO USE Where(s=>s.Deleted==false) CAUSE I M not using base.QUERY()
            IEnumerable<Transactions> favs = domainManager.Query()
                                                                .Where(s => s.Deleted == false)
                                                                .Where(s => s.Sender == shardKey || s.Reciever == shardKey);
            List<MobileTransactions> res  = new List<MobileTransactions>();
            var companiesSet = domainManager.Context.Set<Companies>();
            foreach (var a in favs)
            {
                MobileTransactions temp = Mapper.Map<MobileTransactions>(a);
                if (a.Reciever == shardKey)
                {
                    temp.Money = a.RecieverMoney;
                }
                else
                {
                    temp.Money = a.SenderMoney;
                }
                var cname = companiesSet.Where(s => s.Id == temp.Reciever).FirstOrDefault();
                if (cname != null) temp.Reciever = cname.Name;
                
                var scname = companiesSet.Where(s => s.Id == temp.Sender).FirstOrDefault();
                if (scname != null) temp.Sender = scname.Name;
                
                res.Add(temp);
            }
            return res.AsQueryable<MobileTransactions>();
        }

        public override System.Threading.Tasks.Task<MobileTransactions> UpdateAsync(string id, Delta<MobileTransactions> patch)
        {
            return null;
        }

        public override Task<bool> DeleteAsync(string id)
        {
            return null;
        }
        public override SingleResult<MobileTransactions> Lookup(string id)
        {
            return base.LookupEntity(s => s.Id == id);
        }
        internal void setContext(mpbdmContext<Guid> db)
        {
            this.Context = db;
            this.domainManager.Context = db;
        }
        
    }
}