using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
namespace AllKorrect
{
    sealed class Session : IDisposable
    {
        const uint KEEPALIVE_INTERVAL = 5 * 1000;
        const uint KEEPALIVE_TIME = 60 * 1000;
        TcpClient tcp;
        BinaryReader reader;
        BinaryWriter writer;

        public Session(string host, int port)
        {
            tcp = new TcpClient(host, port);

            //Linger
            tcp.LingerState = new LingerOption(true, 5);

            //Keep-Alive
            byte[] keepAliveValues = new byte[Marshal.SizeOf(typeof(uint)) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(keepAliveValues, 0);
            BitConverter.GetBytes((uint)KEEPALIVE_TIME).CopyTo(keepAliveValues, Marshal.SizeOf(typeof(uint)));
            BitConverter.GetBytes((uint)KEEPALIVE_INTERVAL).CopyTo(keepAliveValues, Marshal.SizeOf(typeof(uint)) * 2);
            tcp.Client.IOControl(IOControlCode.KeepAliveValues, keepAliveValues, null);

            reader = new BinaryReader(tcp.GetStream(), Encoding.ASCII, true);
            writer = new BinaryWriter(tcp.GetStream(), Encoding.ASCII, true);

            writer.Write("POST / HTTP/1.1\r\nHost: www.example.com\r\nUpgrade: AllKorrect\r\n\r\n".ToCharArray());
        }

        public void Send(Message msg)
        {
            msg.Send(writer);
        }

        public Message Receive(MessageType expectedType)
        {
            Message msg = new Message(reader);
            if (msg.Type != expectedType)
            {
                throw new Exception("收到的消息类型不匹配");
            }
            return msg;
        }

        public void Dispose()
        {
            if (tcp.Connected && writer.BaseStream.CanWrite)
            {
                Send(new Message()
                {
                    Type = MessageType.Exit,
                    Body = new byte[0],
                    Size = 0
                });
            }
            tcp.Close();
        }
    }
}
