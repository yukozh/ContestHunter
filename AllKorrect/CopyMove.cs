using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    class CopyMove
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
        public MessageType MsgType { get; set; }
        public Message ToMessage()
        {
            Message msg = new Message();
            msg.Type = MsgType;
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    writer.Write(OldName);
                    writer.Write(NewName);
                }
                msg.Body = mem.ToArray();
            }
            msg.Size = msg.Body.Length;
            return msg;
        }
    }
}
