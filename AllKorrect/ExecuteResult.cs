using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    public class ExecuteResult
    {
        public int ExitStatus;
        public ExecuteResultType Type;
        public string Output, Error;
        public long Memory;
        public int Time;

        internal ExecuteResult(Message msg)
        {
            using (MemoryStream mem = new MemoryStream(msg.Body))
            {
                using (BinaryReader reader = new BinaryReader(mem))
                {
                    ExitStatus = reader.ReadInt32();
                    Type = (ExecuteResultType)reader.ReadInt32();
                    Output = reader.ReadString();
                    Error = reader.ReadString();
                    Memory = reader.ReadInt64();
                    Time = reader.ReadInt32();
                }
            }
        }
    }
}
