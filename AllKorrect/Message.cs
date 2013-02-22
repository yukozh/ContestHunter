using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace AllKorrect
{
    class Message
    {
        const int MAX_BODY_SIZE = 100 * 1024 * 1024;

        public MessageType Type;
        public int Size;
        public byte[] Body;
        public Message() { }
        public Message(BinaryReader reader)
        {
            Type = (MessageType)reader.ReadInt32();
            Size = reader.ReadInt32();
            if (Size > MAX_BODY_SIZE)
            {
                throw new Exception("Message Body Too Large");
            }
            Body = reader.ReadBytes(Size);
            if (Body.Length != Size)
            {
                throw new EndOfStreamException();
            }
        }

        public void Send(BinaryWriter writer)
        {
            writer.Write((int)Type);
            writer.Write(Size);
            writer.Write(Body);
        }
    }
}
