using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truking.CRM.WinSrv.Job;

namespace Truking.CRM.WinSrv
{
    public class QuartzInstance
    {
        private class Nested
        {
            static Nested()
            {
            }
            internal static readonly QuartzInstance _instance = new QuartzInstance();
        }

        public static QuartzInstance Instance { get { return Nested._instance; } }

        /// <summary>
        /// 服务是否开启标志
        /// </summary>
        bool isOpen = true;
        public ISchedulerFactory sf;
        public IScheduler sched;

        private QuartzInstance()
        {
            try
            {
                InitScheduler();
                isOpen = true;
            }
            catch (Exception ex)
            {
                isOpen = false;
            }
        }

        private void InitScheduler()
        {
            Log.Info("system", "定时开启"+ DateTime.Now.ToString());
            try
            {
                sf = new StdSchedulerFactory();
                sched = sf.GetScheduler();
                sched.Start();

                //IJobDetail job3 = JobBuilder.Create<TestJob>().Build();
                //ITrigger trigger3 = TriggerBuilder.Create()
                //   .StartNow()
                //   .WithSimpleSchedule(x => x.WithIntervalInSeconds(3).RepeatForever())
                //   .Build();
                //sched.ScheduleJob(job3, trigger3);
                //sched.Start();

                // .WithSimpleSchedule(x => x.WithIntervalInSeconds(15).RepeatForever())
                IJobDetail job1 = JobBuilder.Create<MatJob>().Build();
                ITrigger trigger1 = TriggerBuilder.Create()
                   .StartNow()
                   .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                   .Build();
                sched.ScheduleJob(job1, trigger1);

                IJobDetail job2 = JobBuilder.Create<SyncOrderJob>().Build();
                ITrigger trigger2 = TriggerBuilder.Create()
                   .StartNow()
                   .WithSimpleSchedule(x => x.WithIntervalInSeconds(60).RepeatForever())
                   .Build();
                sched.ScheduleJob(job2, trigger2);

                IJobDetail job3 = JobBuilder.Create<SyncRecognitionJob>().Build();
                ITrigger trigger3 = TriggerBuilder.Create()
                   .StartNow()
                   .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever())
                   .Build();
                sched.ScheduleJob(job3, trigger3);

                IJobDetail job4 = JobBuilder.Create<SyncRateJob>().Build();
                ITrigger trigger4 = TriggerBuilder.Create()
                   .StartNow()
                   .WithSimpleSchedule(x => x.WithIntervalInHours(12).RepeatForever())
                   .Build();
                sched.ScheduleJob(job4, trigger4);


                IJobDetail job5 = JobBuilder.Create<ProjectmgnJob>().Build();
                ITrigger trigger5 = TriggerBuilder.Create()
                   .StartNow()
                   .WithSimpleSchedule(x => x.WithIntervalInHours(12).RepeatForever())
                   .Build();
                sched.ScheduleJob(job5, trigger5);
            }
            catch (Exception ex)
            {
                Log.Error("system", ex);
            }
        }

        public void GetCurrentlyExecutingJobs()
        {
            sched.GetCurrentlyExecutingJobs();
        }

        public bool Test() 
        {
            return isOpen;
        }
    }
}
