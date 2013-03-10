using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
namespace AllKorrect
{
    sealed class Session : IDisposable
    {
        BlockingCollection<Message> sendQueue = new BlockingCollection<Message>(new ConcurrentQueue<Message>());
        //BlockingCollection<Message> recvQueue = new BlockingCollection<Message>(new ConcurrentQueue<Message>());

        TcpClient tcp;
        BinaryReader reader;
        BinaryWriter writer;
        volatile bool disposing;
        Thread sendThread;
        //Thread recvThread;
        Timer timer;

        public Session(string host, int port)
        {
            tcp = new TcpClient(host, port);
            //tcp.SendTimeout = 5000;
            tcp.ReceiveTimeout = 5000;
            reader = new BinaryReader(tcp.GetStream(), Encoding.ASCII, true);
            writer = new BinaryWriter(tcp.GetStream(), Encoding.ASCII, true);
            sendThread = new Thread(SendingThread);
            sendThread.IsBackground = true;
            sendThread.Start();
            //recvThread = new Thread(ReceiveThread);
            //recvThread.IsBackground = true;
            //recvThread.Start();
            timer = new Timer((a) =>
            {
                Send(new Message()
                {
                    Type = MessageType.IMAlive,
                    Body = new byte[0],
                    Size = 0
                });
            }, null, 3000, 3000);
        }

        public void Send(Message msg)
        {
            sendQueue.Add(msg);
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

        void SendingThread()
        {
            try
            {
                while (true)
                {
                    Message msg = sendQueue.Take();
                    msg.Send(writer);
                }
            }
            catch
            {
                sendQueue.CompleteAdding();
                Dispose();
            }
        }
        /*
        void ReceiveThread()
        {
            try
            {
                while (true)
                {
                    Message msg = new Message(reader);
                    if (msg.Type != MessageType.IMAlive)
                    {
                        recvQueue.Add(msg);
                    }
                }
            }
            catch
            {
                recvQueue.CompleteAdding();
                Dispose();
            }
        }
        */
        public void Dispose()
        {
            if (disposing) return;
            disposing = true;
            timer.Dispose();
            if (tcp.Connected && writer.BaseStream.CanWrite)
            {
                Send(new Message()
                {
                    Type = MessageType.Exit,
                    Body = new byte[0],
                    Size = 0
                });
                //Increase the probability of server to recv the msg
                Thread.Sleep(500);
            }
            tcp.Close();
        }
    }
}
