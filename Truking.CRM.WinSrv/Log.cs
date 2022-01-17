using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truking.CRM.Interface;
using Truking.CRM.WinSrv.Models;

namespace Truking.CRM.WinSrv
{
    public class Log : ILog
    {
        public static void Info(string biz, string msg)
        {
            bizlog log = new bizlog();
            log.lev = "info";
            log.msg = msg;
            log.biz = biz;
            log.ct = DateTime.Now;
            SqliteHelper.Insert(log);
        }

        public static void Error(string biz, Exception e)
        {
            bizlog log = new bizlog();
            log.lev = "error";
            log.msg = e.Message;
            log.biz = biz;
            log.trace = e.StackTrace;
            log.ct = DateTime.Now;
            SqliteHelper.Insert(log);
        }

        public void InfoMsg(string msg)
        {
            bizlog log = new bizlog();
            log.lev = "info";
            log.msg = msg;
            log.biz = "QXB";
            log.ct = DateTime.Now;
            SqliteHelper.Insert(log);
        }

        public void ErrorMsg(string msg)
        {
            bizlog log = new bizlog();
            log.lev = "error";
            log.msg = "";
            log.biz = "QXB";
            log.trace = msg;
            log.ct = DateTime.Now;
            SqliteHelper.Insert(log);
        }
    }
}
