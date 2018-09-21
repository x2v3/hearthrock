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

        private const string InsertPlaySql =
            "insert into playlog (`session`,`turn`,`jsondata`,`action`) values(@session,@turn,@jsondata,@action)";
        private const string InsertErrorSql=
            "insert into errorlog (`session`,`turn`,`jsondata`,`exception`) values(@session,@turn,@jsondata,@exception)";
      

        public int AddPlayLog(RockScene scene, RockAction action)
        {
            if (connection?.State != ConnectionState.Open)
            {
                connection=new MySqlConnection(connString);
                connection.Open();
            }
            var cmd =new MySqlCommand(InsertPlaySql,connection);
            cmd.Parameters.AddWithValue("@session", scene.SessionId);
            cmd.Parameters.AddWithValue("@turn", scene.Turn);
            cmd.Parameters.AddWithValue("@jsondata", JsonConvert.SerializeObject(scene));
            cmd.Parameters.AddWithValue("@action",JsonConvert.SerializeObject(action));
            return cmd.ExecuteNonQuery();
        }

        public Task<int> AddPlayLogAsync(RockScene scene, RockAction action)
        {
            return Task.FromResult(AddPlayLog(scene, action));
        }

        public void AddPlayLogAsyncNoReturn(RockScene scene, RockAction action)
        {
            Task.Run(() => { AddPlayLog(scene, action); });
        }

        public int AddErrorLog(RockScene scene, Exception exception)
        {
            if (connection?.State != ConnectionState.Open)
            {
                connection=new MySqlConnection(connString);
                connection.Open();
            }
            var cmd =new MySqlCommand(InsertPlaySql,connection);
            cmd.Parameters.AddWithValue("@session", scene.SessionId);
            cmd.Parameters.AddWithValue("@turn", scene.Turn);
            cmd.Parameters.AddWithValue("@jsondata", JsonConvert.SerializeObject(scene));
            cmd.Parameters.AddWithValue("@action",JsonConvert.SerializeObject(exception));
            return cmd.ExecuteNonQuery();
        }

        public Task<int> AddErrorAsync(RockScene scene, Exception exception)
        {
            return Task.FromResult(AddErrorLog(scene, exception));
        }

        public void AddErrorLogAsnycNoResult(RockScene scene, Exception exception)
        {
            Task.Run(() => { AddErrorLog(scene, exception); });
        }
    }
    
}
