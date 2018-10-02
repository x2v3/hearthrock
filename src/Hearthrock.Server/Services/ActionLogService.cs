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

        private const string InsertErrorSql =
            "insert into errorlog (`session`,`turn`,`jsondata`,`exception`) values(@session,@turn,@jsondata,@exception)";

        private const string InsertPlayResultSql = "replace into playresult (player,session, win) values (@player,@session,@win);";

        public int AddPlayLog(RockScene scene, RockAction action)
        {
            if (connection?.State != ConnectionState.Open)
            {
                connection = new MySqlConnection(connString);
                connection.Open();
            }
            var cmd = new MySqlCommand(InsertPlaySql, connection);
            cmd.Parameters.AddWithValue("@session", scene.SessionId);
            cmd.Parameters.AddWithValue("@turn", scene.Turn);
            cmd.Parameters.AddWithValue("@jsondata", JsonConvert.SerializeObject(scene));
            cmd.Parameters.AddWithValue("@action", JsonConvert.SerializeObject(action));
            return cmd.ExecuteNonQuery();
        }

        public async Task<int> AddPlayLogAsync(RockScene scene, RockAction action)
        {
            return await Task.FromResult(AddPlayLog(scene, action));
        }

        public void AddPlayLogAsyncNoReturn(RockScene scene, RockAction action)
        {
            Task.Run(() => { AddPlayLog(scene, action); });
        }

        public int AddErrorLog(RockScene scene, Exception exception)
        {
            if (connection?.State != ConnectionState.Open)
            {
                connection = new MySqlConnection(connString);
                connection.Open();
            }
            var cmd = new MySqlCommand(InsertErrorSql, connection);
            cmd.Parameters.AddWithValue("@session", scene.SessionId);
            cmd.Parameters.AddWithValue("@turn", scene.Turn);
            cmd.Parameters.AddWithValue("@jsondata", JsonConvert.SerializeObject(scene));
            try
            {
                cmd.Parameters.AddWithValue("@exception", JsonConvert.SerializeObject(exception));
            }
            catch (Exception e)
            {
                cmd.Parameters.AddWithValue("@exception", DBNull.Value);
                Console.WriteLine($"Write error log to db error:{e.Message}");
            }
            return cmd.ExecuteNonQuery();
        }

        public async Task<int> AddErrorAsync(RockScene scene, Exception exception)
        {
            return await Task.FromResult(AddErrorLog(scene, exception));
        }

        public void AddErrorLogAsnycNoResult(RockScene scene, Exception exception)
        {
            Task.Run(() => { AddErrorLog(scene, exception); });
        }

        public int AddPlayResult(PlayResult result)
        {
            if (connection?.State != ConnectionState.Open)
            {
                connection = new MySqlConnection(connString);
                connection.Open();
            }
            var cmd = new MySqlCommand(InsertPlayResultSql, connection);
            cmd.Parameters.AddWithValue("@player", result.PlayerName);
            cmd.Parameters.AddWithValue("@session", result.Session);
            cmd.Parameters.AddWithValue("@win", result.Won);
            return cmd.ExecuteNonQuery();
        }

        public async Task<int> AddPlayResultAsync(PlayResult result)
        {
            return await Task.FromResult(AddPlayResult(result));
        }

        public void AddPlayResultAsyncNoResult(PlayResult result)
        {
            Task.Run(() => { AddPlayResult(result); });
        }

        public IEnumerable<dynamic> GetPlaySummary()
        {
            var sql = @"select player,count(*) as total,sum(win) as win,sum(win)/count(*) as rate from playresult
            where logtime>current_date()
            group by player";
            if (connection == null)
            {
                connection = new MySqlConnection(connString);
                connection.Open();
            }
            var da = new MySqlDataAdapter(sql, connection);
            var dt = new DataTable();
            da.Fill(dt);
            var lst = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                lst.Add(new
                {
                    Player = Convert.ToString(row["player"]),
                    Total = Convert.ToString(row["total"]),
                    Win = Convert.ToString(row["win"]),
                    Rate = Convert.ToString(row["rate"]),
                });
            }

            return lst;
        }
    }
}