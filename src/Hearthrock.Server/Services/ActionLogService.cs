using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Hearthrock.Server.Services
{
    public class ActionLogService
    {
        public ActionLogService(string connstr)
        {
            connString = connstr;
        }

        private MySqlConnection connection;
        private string connString;
        private const string InsertSql ="insert into playlog (`session`,`turn`,`jsondata`,`action`) values(@session,@turn,@jsondata,@action)"
      

        public int AddPlayLog(RockScene scene, RockAction action)
        {
            if (connection?.State != ConnectionState.Open)
            {
                connection=new MySqlConnection(connString);
                connection.Open();
            }
            var cmd =new MySqlCommand(InsertSql,connection);
            cmd.Parameters.AddWithValue("@session", scene.SessionId);
            cmd.Parameters.AddWithValue("@turn", scene.Turn);
            cmd.Parameters.AddWithValue("@jsondata", JsonConvert.SerializeObject(scene));
            cmd.Parameters.AddWithValue("@action",JsonConvert.SerializeObject(action));
            return cmd.ExecuteNonQuery();
        }
    }
}
