using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            SimpleCRUD.SetDialect(SimpleCRUD.Dialect.SQLite);
            QuartzInstance.Instance.Test();
        }

        protected override void OnStop()
        {
        }
    }
}
