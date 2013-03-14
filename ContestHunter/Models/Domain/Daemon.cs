using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.IO;

namespace ContestHunter.Models.Domain
{
    public abstract class Daemon
    {
        Thread thread;
        volatile bool running;
        public volatile Exception LastException;
        public volatile DateTime ExceptionTime;
        public enum StatusType
        {
            Running,
            Stopped,
            Crashed
        }
        public volatile StatusType type;

        protected abstract int Run();

        public void Start()
        {
            if (thread != null)
            {
                Stop();
            }

            running = true;
            thread = new Thread(ThreadMain);
            thread.Start();
        }

        public void Stop()
        {
            running = false;
            thread.Interrupt();
            thread.Join();
            thread = null;
        }

        public void ThreadMain()
        {
            while (running)
            {
                try
                {
                    LastException = null;
                    type = StatusType.Running;
                    int toSleep = Run();
                    type = StatusType.Stopped;
                    Thread.Sleep(toSleep);
                }
                catch (Exception e)
                {
                    type = StatusType.Crashed;
                    LastException = e;
                    ExceptionTime = DateTime.Now;
                    if (!running)
                    {
                        break;
                    }
                    else
                    {
                        LogHelper.WriteLog(GetType().Name, e.ToString());
                        try
                        {
                            
                            //Sleep a while to prevent fill the disk
                            Thread.Sleep(10000);
                        }
                        catch { }
                    }
                }
            }
        }

    }
}