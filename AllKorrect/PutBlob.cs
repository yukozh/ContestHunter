using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    class PutBlob
    {
        public string Name;
        public int Size;
        public byte[] Bytes;

        public Message ToMessage()
        {
            Message msg = new Message();
            msg.Type = MessageType.PutBlob;
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    writer.Write(Name);
                    writer.Write(Size);
                    writer.Write(Bytes);
                }
                msg.Body = mem.ToArray();
            }
            msg.Size = msg.Body.Length;
            return msg;
        }
    }
}
