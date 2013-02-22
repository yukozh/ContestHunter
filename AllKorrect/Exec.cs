using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    class Exec
    {
        public string Command;
        public int ArgumentCount;
        public IEnumerable<string> Arguments;
        public long MemoryLimit;
        public long OutputLimit;
        public int TimeLimit;
        public RestrictionLevel Restriction;
        public string InputBlob;

        public Message ToMessage()
        {
            Message msg = new Message();
            msg.Type = MessageType.Execute;
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    writer.Write(Command);
                    writer.Write(ArgumentCount);
                    foreach (var arg in Arguments)
                    {
                        writer.Write(arg);
                    }
                    writer.Write(MemoryLimit);
                    writer.Write(OutputLimit);
                    writer.Write(TimeLimit);
                    writer.Write((int)Restriction);
                    writer.Write(InputBlob);
                }
                msg.Body = mem.ToArray();
            }
            msg.Size = msg.Body.Length;
            return msg;
        }
    }
}
