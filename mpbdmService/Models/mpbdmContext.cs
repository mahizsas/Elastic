using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Tables;
using mpbdmService.DataObjects;
using System.Data.Entity.Validation;
using System;
using System.Data.SqlClient;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using System.Data.Common;
using System.Diagnostics;

namespace mpbdmService.Models
{
    public class mpbdmContext<T> : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to alter your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
        //
        // To enable Entity Framework migrations in the cloud, please ensure that the 
        // service name, set by the 'MS_MobileServiceName' AppSettings in the local 
        // Web.config, is the same as the service name when hosted in Azure.
        private const string connectionStringName = "Name=ElasticConnectionString";

        public mpbdmContext() : base(connectionStringName)
        {
        } 


        public DbSet<Contacts> Contacts { get; set; }

        public DbSet<Groups> Groups { get; set; }

        public DbSet<Companies> Companies { get; set; }

        public DbSet<Favorites> Favorites { get; set; }

        public DbSet<Users> Users { get; set; }

        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            string schema = ServiceSettingsDictionary.GetSchemaName();
            if (!string.IsNullOrEmpty(schema))
            {
                modelBuilder.HasDefaultSchema(schema);
            }

            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }

        //public System.Data.Entity.DbSet<mpbdmService.DTO.MobileGroup> MobileGroups { get; set; }

        //public System.Data.Entity.DbSet<mpbdmService.DataObjects.MobileFavorites> MobileFavorites { get; set; }
        #region ElasticScale Needed

        protected internal mpbdmContext(string connectionString)
            : base(SetInitializerForConnection(connectionString))
        {
        }

        // Only static methods are allowed in calls into base class c'tors
        private static string SetInitializerForConnection(string connnectionString)
        {
            // We want existence checks so that the schema can get deployed
            Database.SetInitializer<mpbdmContext<Guid>>(new CreateDatabaseIfNotExists<mpbdmContext<Guid>>());
            return connnectionString;
        }

        // C'tor for data dependent routing. This call will open a validated connection routed to the proper
        // shard by the shard map manager. Note that the base class c'tor call will fail for an open connection
        // if migrations need to be done and SQL credentials are used. This is the reason for the 
        // separation of c'tors into the DDR case (this c'tor) and the internal c'tor for new shards.
        public mpbdmContext(ShardMap shardMap, T shardingKey, string connectionStr)
            : base(CreateDDRConnection(shardMap, shardingKey, connectionStr), true /* contextOwnsConnection */)
        {
        }

        // Only static methods are allowed in calls into base class c'tors
        private static DbConnection CreateDDRConnection(ShardMap shardMap, T shardingKey, string connectionStr)
        {
            // No initialization
            Database.SetInitializer<mpbdmContext<T>>(null);

            // Ask shard map to broker a validated connection for the given key
            SqlConnection conn = shardMap.OpenConnectionForKey<T>(shardingKey, connectionStr, ConnectionOptions.Validate);

            return conn;
        }

        #endregion
    }

}
