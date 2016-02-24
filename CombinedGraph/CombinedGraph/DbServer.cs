using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedGraph
{
    public class ServerData
    {
        public string ServerAdress { get; set; }
        // public string Port_Number { get; set; }
        public string NameDatabase { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
    }
    public class DbServer
    {
        public static string connString;
        //public static string connString
        //{
        //    get; set;
        //}

        public void Db_Init(ServerData data)
        {
            //SqlConnection sql;
            const string providerName = "MySql.Data.MySqlClient";
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.Password = data.Password;
            builder.UserID = data.UserName;
            builder.InitialCatalog = data.NameDatabase;
            builder.DataSource = data.ServerAdress;
      
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            entityBuilder.Provider = providerName;
            entityBuilder.Metadata = "res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl";
            entityBuilder.ProviderConnectionString = builder.ToString();
          connString = entityBuilder.ConnectionString;
           
        }
    }
}
