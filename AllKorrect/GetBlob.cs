using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    class GetBlob
    {
        public string Name;
        public Message ToMessage()
        {
            Message msg = new Message();
            msg.Type = MessageType.GetBlob;
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    writer.Write(Name);
                }
                msg.Body = mem.ToArray();
            }
            msg.Size = msg.Body.Length;
            return msg;
        }
    }
}
