using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace ContestHunter.Models.Domain
{
    public abstract class Daemon
    {
        public static void StartAll()
        {
        }

        public static void StopAll()
        {
        }

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
                    if (e is ThreadInterruptedException)
                    {
                        //Going to Stop
                    }
                    else
                    {
                        //Sleep a while to prevent fill the disk
                        Thread.Sleep(10000);
                    }
                }
            }
        }
    }
}