using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthrock.Contracts;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Hearthrock.Server.ML
{
    public class MysqlDbDataReader 
    {
        public MysqlDbDataReader(string connStr)
        {
            connectionString = connStr;
        }

        public MysqlDbDataReader(string server, string user, string password, string dbName)
            : this($"server={server};uid={user};pwd={password};database={dbName};sslmode=none")
        {

        }

        private string connectionString;

        public List<Match> GetMatches()
        {
            return GetMatches(100, 0);
        }

        public List<Match> GetMatches(int limit, int offset)
        {
            var matchlist = new List<Match>();
            var sql = $"select * from playresult order by id desc limit {limit} offset {offset}";
            var ds = MySqlHelper.ExecuteDataset(connectionString, sql);
            var dt = ds.Tables[0];
            var tasks = new List<Task>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                var match = new Match()
                {
                    Id = Convert.ToInt32(row["id"]),
                    Win = Convert.ToBoolean(row["win"]),
                    Session = Convert.ToString(row["session"])
                };
                matchlist.Add(match);
                var t = Task.Run(() =>
                {
                    match.Turns = GetTurnsByMatchSession(match.Session);
                });
                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());
            return matchlist;
        }

        public List<RockScene> GetTurnsByMatchSession(string session)
        {
            var turnsList = new List<RockScene>();
            try
            {

                var sql = @"select log.* from playlog log join (select session,turn,min(id) as id from playlog where session=@session group by session,turn) as a
            on a.id=log.id
            order by id desc";
                var ds = MySqlHelper.ExecuteDataset(connectionString, sql,
                    new MySqlParameter("@session", MySqlDbType.VarChar) { Value = session });
                var dt = ds.Tables[0];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var turnData = Convert.ToString(dt.Rows[i]["jsondata"]);
                    var scene = JsonConvert.DeserializeObject<RockScene>(turnData);
                    turnsList.Add(scene);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"error load data for session {session}:{e.Message}");
            }

            return turnsList;
        }
        
    }
}
