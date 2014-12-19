/*
    Copyright 2014 Microsoft, Corp.

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System.Data.SqlClient;
using System.Linq;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using mpbdmService.Models;
using Microsoft.ServiceBus;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.Data;
using System.Security.Principal;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;


namespace mpbdmService.ElasticScale
{
    public class Sharding
    {

        public static string server = "tcp:xvgrmio1y8.database.windows.net,1433";//ConfigurationManager.ConnectionStrings["ElasticServer"].ToString();
        public static string shardmapmgrdb = "shardmap";
        public static string connectionString = ConfigurationManager.ConnectionStrings["ElasticConnectionString"].ToString();

        public  string connstring = "";
        private const string sharmapName = "mpbdmService";
        public ShardMapManager ShardMapManager { get; private set; }

        public ListShardMap<Guid> ShardMap { get; private set; }

        public static string FindShard(IPrincipal User)
        {
            ClaimsIdentity claimsUser = User.Identity as ClaimsIdentity;
            Claim customProperties = claimsUser.FindFirst("urn:microsoft:credentials");
            JObject jObject = JObject.Parse(customProperties.Value);

            return jObject["shardKey"].ToString();
        }

        public Shard FindRoomForCompany() {
            Shard shard = null;
            
            IEnumerable<Shard> shards = ShardMap.GetShards();
            foreach( Shard temp in shards ){
                if (ShardMap.GetMappings(temp).Count < 3)
                {
                    return temp;
                }
            }
            return shard;
        }
        // Bootstrap Elastic Scale by creating a new shard map manager and a shard map on 
        // the shard map manager database if necessary.
        public Sharding()//string smmserver, string smmdatabase, string smmconnstr)
        {
            string smmserver = Sharding.server;
            string smmconnstr = Sharding.connectionString;
            string smmdatabase = Sharding.shardmapmgrdb;
            // Connection string with administrative credentials for the root database
            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(smmconnstr);
            connStrBldr.DataSource = smmserver;
            connStrBldr.InitialCatalog = smmdatabase;

            connstring = smmconnstr;

            // Deploy shard map manager.
            ShardMapManager smm;
            if (!ShardMapManagerFactory.TryGetSqlShardMapManager(connStrBldr.ConnectionString, ShardMapManagerLoadPolicy.Lazy, out smm))
            {
                this.ShardMapManager = ShardMapManagerFactory.CreateSqlShardMapManager(connStrBldr.ConnectionString);
            }
            else
            {
                this.ShardMapManager = smm;
            }

            ListShardMap<Guid> sm;
            if (!ShardMapManager.TryGetListShardMap<Guid>(sharmapName, out sm))
            {
                this.ShardMap = ShardMapManager.CreateListShardMap<Guid>(sharmapName);
            }
            else
            {
                this.ShardMap = sm;
            }


            initDd("shard0");
            initDd("shard1");
            //initDd("shard2");
            //initDd("shard3");
            //initDd("shard4"); 
        }
        private void initDd(string database)
        {
            Shard temp;
            if (!this.ShardMap.TryGetShard(new ShardLocation(server, database), out temp))
            {
                temp = this.ShardMap.CreateShard(new ShardLocation(server, database));
            }

            string connstr = ConfigurationManager.ConnectionStrings["ElasticConnectionString"].ConnectionString;
            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(connstr);
            connStrBldr.DataSource = server;
            connStrBldr.InitialCatalog = database;

            // Go into a DbContext to trigger migrations and schema deployment for the new shard.
            // This requires an un-opened connection.
            using (var db = new mpbdmContext<Guid>(connStrBldr.ConnectionString))
            {
                // Run a query to engage EF migrations
                (from b in db.Companies
                 select b).Count();
            }
        }
        // Enter a new shard - i.e. an empty database - to the shard map, allocate a first tenant to it 
        // and kick off EF intialization of the database to deploy schema
        // public void RegisterNewShard(string server, string database, string user, string pwd, string appname, int key)
        public void RegisterNewShard(string server, string database, string connstr, string key)
        {
            Shard shard;
            ShardLocation shardLocation = new ShardLocation(server, database);

            if (!this.ShardMap.TryGetShard(shardLocation, out shard))
            {
                shard = this.ShardMap.CreateShard(shardLocation);
            }

            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(connstr);
            connStrBldr.DataSource = server;
            connStrBldr.InitialCatalog = database;

            // Go into a DbContext to trigger migrations and schema deployment for the new shard.
            // This requires an un-opened connection.
            using (var db = new mpbdmContext<Guid>(connStrBldr.ConnectionString))
            {
                // Run a query to engage EF migrations
                (from b in db.Companies
                 select b).Count();
            }

            // Register the mapping of the tenant to the shard in the shard map.
            // After this step, DDR on the shard map can be used
            PointMapping<Guid> mapping;
            if (!this.ShardMap.TryGetMappingForKey(new Guid(key), out mapping))
            {
                this.ShardMap.CreatePointMapping(new Guid(key), shard);
            }
        }

        public void RegisterNewShard(string database, string key)
        {
            this.RegisterNewShard(Sharding.server , database , Sharding.connectionString , key);
        }
    }

    internal static class SqlDatabaseUtils
    {
        /// <summary>
        /// Gets the retry policy to use for connections to SQL Server.
        /// </summary>
        public static Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy SqlRetryPolicy
        {
            get { return new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(10, TimeSpan.FromSeconds(5)); }
        }
    }
}
