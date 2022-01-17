using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv
{
    public class SqliteHelper
    {
        /// <summary>
        /// BW客户端连接串
        /// </summary>
        public static string connStr
        {
            get
            {
                var file = AppDomain.CurrentDomain.BaseDirectory + @"\truking.db";
                if (!File.Exists(file))
                {
                    SQLiteConnection.CreateFile(file);
                }
                return @"Data Source = " + file + @";Initial Catalog=sqlite;Integrated Security=True;Max Pool Size=10";
            }
        }


        public static int ExecuteNonQuery_Dapper(string sqlStr, object pars = null)
        {
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            var rv = Connection.Execute(sqlStr, pars);
            return rv;
        }

        public static T ExecuteScalar<T>(string sqlStr, object pars = null)
        {
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            var rv = Connection.ExecuteScalar<T>(sqlStr, pars);
            return rv;
        }

        public static int? Insert<T>(T data)
        {
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            return Connection.Insert(data);
        }

        public static T GetObject<T>(string id, string tablename)
        {
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            var rv = Connection.Query<T>("select * from " + tablename + " where id=" + id).FirstOrDefault();
            return rv;
        }

        public static List<T> GetList<T>(string ids, string tablename)
        {
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            if (string.IsNullOrEmpty(ids))
            {
                var rv = Connection.Query<T>("select * from " + tablename).ToList();
                return rv;
            }
            else
            {
                var rv = Connection.Query<T>("select * from " + tablename + " where id in (" + ids + ")").ToList();
                return rv;
            }
        }

        public static List<T> GetList2<T>(string tablename, string wherecase)
        {
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            if (string.IsNullOrEmpty(wherecase))
            {
                var rv = Connection.Query<T>("select * from " + tablename).ToList();
                return rv;
            }
            else
            {
                var rv = Connection.Query<T>("select * from " + tablename + " where " + wherecase).ToList();
                return rv;
            }
        }

        public static void SetConfig(string key, string value)
        { 
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            object user = Connection.ExecuteScalar("select ckey from config where ckey=@ckey", new { ckey = key.ToString() });
            var pars2 = new
            {
                ckey = key.ToString(),
                cvalue = value
            };
            if (user == null)
            {
                Connection.Execute("insert into config(ckey,cvalue) values(@ckey,@cvalue)", pars2);
            }
            else
            {
                Connection.Execute("update config set cvalue=@cvalue where ckey=@ckey", pars2);
            }
        }

        public static string GetConfig(string key)
        {
            SQLiteConnection Connection = new SQLiteConnection(connStr);
            object rv = Connection.ExecuteScalar("select cvalue from config where ckey=@ckey", new { ckey = key.ToString() });
            if (rv != null)
            {
                return rv.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
