using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Truking.CRM.WinSrv.Job
{
    [DisallowConcurrentExecution]
    public class TestJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Log.Info("TestJob", DateTime.Now.ToString());
        }
    }
}
