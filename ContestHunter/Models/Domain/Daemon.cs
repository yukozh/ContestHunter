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
                    Thread.Sleep(Run());
                }
                catch (Exception e)
                {
                    if (!running)
                    {
                        break;
                    }
                    else
                    {
                        try
                        {
                            string logItem=string.Format("LOG:{0} {1}:\r\n{2}\r\n",DateTime.Now,this.GetType().Name,e.ToString());
                            File.AppendAllText(Path.Combine(Framework.WebRoot, "App_Data\\Daemon.log"), logItem);
                        }
                        catch { }
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