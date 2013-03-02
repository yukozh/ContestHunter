using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    class GetBlobParticle
    {
        public string Name;
        public int Skip;
        public int Top;

        public Message ToMessage()
        {
            Message msg = new Message();
            msg.Type = MessageType.GetBlobParticle;
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    writer.Write(Name);
                    writer.Write(Skip);
                    writer.Write(Top);
                }
                msg.Body = mem.ToArray();
            }
            msg.Size = msg.Body.Length;
            return msg;
        }
    }
}
