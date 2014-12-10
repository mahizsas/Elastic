using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
using mpbdmService.Models;
using AutoMapper;
using mpbdmService.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using mpbdmService.ElasticScale;
using System.Configuration;

namespace mpbdmService
{
    public static class WebApiConfig
    {
        public static Sharding ShardingObj;
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            //config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            //var formatters = config.Formatters;
            //var jsonFormatter = formatters.JsonFormatter;
            //var settings = jsonFormatter.SerializerSettings;
            //settings.Formatting = Formatting.Indented;
            //settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Elastic Scale must ensure data consistency
            //Database.SetInitializer(new mpbdmInitializer());
            
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Groups, MobileGroup>().ReverseMap();
                cfg.CreateMap<Favorites, MobileFavorites>().ReverseMap();
            });


            // Sharding specific stuff
            string server = "tcp:hx7y5ohy6t.database.windows.net,1433";
            string shardmapmgrdb = "shardmap";
            string connectionString = ConfigurationManager.ConnectionStrings["ElasticConnectionString"].ToString();
            string shard1 = "shard0";
            string shard2 = "shard1";
            string company1 = "48344df7-4837-4144-b1c8-21203bea780d";
            string company2 = "2c8c7462-d6ca-429c-9021-21203bea780d";

            // sharding GLOBAL 
            ShardingObj = new Sharding(server, shardmapmgrdb, connectionString);
            ShardingObj.RegisterNewShard(server, shard1, connectionString, company1);
            ShardingObj.RegisterNewShard(server, shard2, connectionString, company2);

            // Register the mapping of the tenant to the shard in the shard map.
            // After this step, DDR on the shard map can be used
            /*
            PointMapping<int> mapping;
            if (!this.ShardMap.TryGetMappingForKey(key, out mapping))
            {
                this.ShardMap.CreatePointMapping(key, shard);
            }
            //*/
        }
    }

    public class mpbdmInitializer : ClearDatabaseSchemaIfModelChanges<mpbdmContext<Guid>> 
                                    //ClearDatabaseSchemaAlways<mpbdmContext<Guid>>
    {
        protected override void Seed(mpbdmContext<Guid> context)
        {
        

            /*
            * COMPANIES
            */
            string[] companiesArray = new string[2];
            int count_companies = 0;
            List<Companies> companies = new List<Companies>
            {
                new Companies { Id = "2c8c7462-d6ca-429c-9021-21203bea780d", Name = "Sieben", Address = "Αθήνα" , Email = "sieben@sieben.gr"},
                new Companies { Id = "48344df7-4837-4144-b1c8-6470aeb9dae4", Name = "Coca-Cola", Address = "Αθήνα", Email = "coca@cola.com"},
            };

            foreach (Companies company in companies)
            {
                companiesArray[count_companies] = company.Id;
                context.Set<Companies>().Add(company);
                count_companies++;
            }

            
            /*
             * USERS
             */
            Users[] usersArray = new Users[5];
            int count_users = 0;
            List<Users> users = new List<Users>
            {
                new Users { Id = "Google:105535740556221909032", FirstName = "Στέφανος", LastName = "Λιγνός" , Email = "s.lignos@sieben.gr", CompaniesID = companiesArray[0]},
                new Users { Id = "Google:108551266495594343585", FirstName = "Μάνος", LastName = "Ψαράκης", Email = "s.psarakis@sieben.gr", CompaniesID = companiesArray[1]},
                new Users { Id = "Facebook:762253580534078", FirstName = "Nikos", LastName = "Atlas", Email = "s.psarakis@sieben.gr", CompaniesID = companiesArray[0]},
                new Users { Id = "custom:nikatlas", FirstName = "Nikatlas", LastName = "Atlas", Email = "n.atlas@sieben.gr", CompaniesID = companiesArray[0]},
                new Users { Id = "custom:steflignos", FirstName = "Stefanos", LastName = "Lignos", Email = "n.atlas@sieben.gr", CompaniesID = companiesArray[0]},
            };

            foreach (Users user in users)
            {
                usersArray[count_users] = user;
                context.Set<Users>().Add(user);
                count_users++;
            }

            byte[] salt = CustomLoginProviderUtils.generateSalt();
            Account nik = new Account
            {
                Id = Guid.NewGuid().ToString(),
                Username = "nikatlas",
                Salt = salt,
                SaltedAndHashedPassword = CustomLoginProviderUtils.hash("123321qwe", salt),
                User = usersArray[3]
            };
            Account stef = new Account
            {
                Id = Guid.NewGuid().ToString(),
                Username = "steflignos",
                Salt = salt,
                SaltedAndHashedPassword = CustomLoginProviderUtils.hash("123321qwe", salt),
                User = usersArray[4]
            };

            List<Account> accs = new List<Account>
            {
                nik,stef
            };

            foreach (Account acc in accs)
            {
                context.Set<Account>().Add(acc);
            }

            /*
             * GROUPS
             */
            string[] groupsArray = new string[8];
            int count_groups = 0;
            List<Groups> groups = new List<Groups>
            {
                new Groups { Id = Guid.NewGuid().ToString(), Name = "Research & development", Address = "Αθήνα", Visible = true, CompaniesID = companiesArray[0]},
                new Groups { Id = Guid.NewGuid().ToString(), Name = "Digital Marketing", Address = "Αθήνα", Visible = true, CompaniesID = companiesArray[0]},
                new Groups { Id = Guid.NewGuid().ToString(), Name = "Human Resources", Address = "Αθήνα", Visible = true, CompaniesID = companiesArray[0]},
                new Groups { Id = Guid.NewGuid().ToString(), Name = "Sales & Marketing", Address = "Αθήνα", Visible = true, CompaniesID = companiesArray[0]},
                new Groups { Id = Guid.NewGuid().ToString(), Name = "Other", Address = "Στο Πουθενά", Visible = true, CompaniesID = companiesArray[0]},
                new Groups { Id = Guid.NewGuid().ToString(), Name = "Human Resources", Address = "Αθήνα", Visible = true, CompaniesID = companiesArray[1]},
                new Groups { Id = Guid.NewGuid().ToString(), Name = "Sales & Marketing", Address = "Αθήνα", Visible = true, CompaniesID = companiesArray[1]},
                new Groups { Id = Guid.NewGuid().ToString(), Name = "None", Address = "Στο Πουθενά", Visible = true, CompaniesID = companiesArray[1]},
            };

            foreach (Groups group in groups)
            {
                groupsArray[count_groups] = group.Id;
                context.Set<Groups>().Add(group);
                count_groups++;
            }


            /*
             * CONTACTS
             */
            string[] contactsArray = new string[22];
            int count_contacts = 0;
            List<Contacts> contacts = new List<Contacts>
            {
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Μαρία", LastName = "Κουνάκη", Phone = "6974767832", Email = "m.kounaki@sieben.gr", Visible = true , GroupsID = groupsArray[1] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Φίλιππος", LastName = "Κολέτσης", Phone = "6973245684", Email = "f.koletsis@sieben.gr", Visible = true , GroupsID = groupsArray[1] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Γιώργος", LastName = "Αργυράκης", Phone = "6974532123", Email = "g.argurakis@sieben.gr", Visible = true , GroupsID = groupsArray[1] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Μάρα", LastName = "Κυμπιζη", Phone = "6932456789", Email = "m.kumpizh@sieben.gr", Visible = true , GroupsID = groupsArray[1] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Κωνσταντίνος", LastName = "Τζαβάρας", Phone = "697356745", Email = "k.tzavaras@sieben.gr", Visible = true , GroupsID = groupsArray[3] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Αντωνία", LastName = "Ρεμούνδου", Phone = "6975634251", Email = "a.remoundou@sieben.gr", Visible = true , GroupsID = groupsArray[3] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Ολγα", LastName = "Ζούμπου", Phone = "6934567324", Email = "o.zoumpou@sieben.gr", Visible = true , GroupsID = groupsArray[3] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Νικόλ", LastName = "Μπουζούκου", Phone = "6943245622", Email = "n.mpouzoukou@sieben.gr", Visible = true , GroupsID = groupsArray[2] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Στέφανος", LastName = "Λιγνός", Phone = "6974053682", Email = "s.lignos@sieben.gr", Visible = true , GroupsID = groupsArray[0] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Νίκος", LastName = "Καλαβρουζιώτης", Phone = "6982108999", Email = "n.kalavrouziotis@sieben.gr", Visible = true , GroupsID = groupsArray[0] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Φοίβος", LastName = "Σταμόπουλος", Phone = "6945849202", Email = "f.stamopoulos@sieben.gr", Visible = true , GroupsID = groupsArray[0] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Μάνος", LastName = "Ψαράκης", Phone = "6942466270", Email = "m.psarakis@sieben.gr", Visible = true , GroupsID = groupsArray[7] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Βασιλική", LastName = "Τραχάνη", Phone = "6938627519", Email = "b.trachani@sieben.gr", Visible = true , GroupsID = groupsArray[0] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Ελένη", LastName = "Παπανικολάου", Phone = "6943567743", Email = "e.papanikolaou@inedu.gr", Visible = true , GroupsID = groupsArray[0] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Γιώργος", LastName = "Σαχπατζίδης", Phone = "6945823948", Email = "g.sachpatzidis@sieben.gr", Visible = true, GroupsID = groupsArray[0] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Κωνσταντίνα", LastName = "Παπαδοπούλου", Phone = "6981222331", Email = "k.papadopoulou@coca.gr", Visible = true , GroupsID = groupsArray[5] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Γιάννης", LastName = "Παντζόπουλος", Phone = "6974567342", Email = "g.pantzopoulos@coca.gr", Visible = true , GroupsID = groupsArray[5] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Γιάννης", LastName = "Ρέγκας", Phone = "6946578423", Email = "g.regkas@coca.gr" , Visible = true, GroupsID = groupsArray[5] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Μαρία", LastName = "Σκαλκούτα", Phone = "6933745862", Email = "m.skalkouta@coca.gr" , Visible = true, GroupsID = groupsArray[5] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Κωνσταντίνος", LastName = "Τζάνης", Phone = "6975678421", Email = "k.tzanhs@coca.gr", Visible = true , GroupsID = groupsArray[6] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Αντωνία", LastName = "Ρέβη", Phone = "6981234767", Email = "a.revh@coca.gr", Visible = true , GroupsID = groupsArray[6] },
                new Contacts { Id = Guid.NewGuid().ToString(), FirstName = "Όλγα", LastName = "Ζούνη", Phone = "6943567422", Email = "o.zounh@coca.gr", Visible = true , GroupsID = groupsArray[6] },
            };

            foreach (Contacts contact in contacts)
            {
                contactsArray[count_contacts] = contact.Id;
                context.Set<Contacts>().Add(contact);
                count_contacts++;
            }


            /*
             * FAVORITES
             */
            List<Favorites> favorites = new List<Favorites>
            {
                new Favorites { Id = Guid.NewGuid().ToString(), Visible = true , UsersID = usersArray[0].Id, ContactsID = contactsArray[0]},
                new Favorites { Id = Guid.NewGuid().ToString(), Visible = true , UsersID = usersArray[3].Id, ContactsID = contactsArray[1]},
                new Favorites { Id = Guid.NewGuid().ToString(), Visible = true , UsersID = usersArray[4].Id, ContactsID = contactsArray[7]},
                new Favorites { Id = Guid.NewGuid().ToString(), Visible = true , UsersID = usersArray[0].Id, ContactsID = contactsArray[8]},
                new Favorites { Id = Guid.NewGuid().ToString(), Visible = true , UsersID = usersArray[1].Id, ContactsID = contactsArray[16] },
                
            };

            foreach (Favorites favorite in favorites)
            {
                context.Set<Favorites>().Add(favorite);
            }





            base.Seed(context);
        }
    }
}

