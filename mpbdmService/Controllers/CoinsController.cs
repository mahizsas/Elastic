using Microsoft.WindowsAzure.Mobile.Service.Security;
using mpbdmService.ElasticScale;
using mpbdmService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace mpbdmService.Controllers
{
    // MAYBE REMOVE THIS CONTROLLER FOR Beaty!
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class CoinsController : ApiController
    {
        // IMPLEMENTED IN COMPANIES VIA ROUTING 
        public HttpResponseMessage Get()
        {
            
            string shardKey = Sharding.FindShard(User);
            mpbdmContext<Guid> db = new mpbdmContext<Guid>(WebApiConfig.ShardingObj.ShardMap, new Guid(shardKey), WebApiConfig.ShardingObj.connstring);

            var coins = getRecieverMoney(shardKey, db);
            return Request.CreateResponse(HttpStatusCode.OK , coins);
        }
        public static double getRecieverMoney(string shardKey, mpbdmContext<Guid> db)
        {
            var res = db.Transactions.Where(s => s.Status == "Completed" && (s.Sender == shardKey || s.Reciever == shardKey)).OrderByDescending(s => s.UpdatedAt).FirstOrDefault();
            if (res != null)
            {
                if (res.Reciever == shardKey) return res.RecieverMoney;
                else return res.SenderMoney;
            }
            return 0.0;
        }
    }
}
