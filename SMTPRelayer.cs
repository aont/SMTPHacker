using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace SMTPHacker
{
    class SMTPRelayer : IDisposable
    {
        TcpClient client_tcp;
        NetworkStream client_Stream;

        TcpClient server_tcp;
        NetworkStream server_Stream;

        Thread readthread;
        Thread writethread;
        SMTPSettings settings;


        public SMTPRelayer(TcpClient tcpClient, SMTPSettings settings)
        {
            this.settings = settings;
            Console.WriteLine("Connected... {0}", tcpClient.GetHashCode());

            this.client_tcp = tcpClient;
            this.client_Stream = tcpClient.GetStream();

            this.server_tcp = new TcpClient(settings.RemoteAddress, settings.RemotePort);
            this.server_Stream = server_tcp.GetStream();


            this.readthread = new Thread(new ThreadStart(Read));
            this.writethread = new Thread(new ThreadStart(Write));

        }

        public void Start()
        {
            this.readthread.Start();
            this.writethread.Start();
        }

        public void Dispose()
        {
            if (writethread.IsAlive)
                writethread.Abort();
            if (readthread.IsAlive)
                readthread.Abort();
            this.client_tcp.Close();
            this.server_tcp.Close();
        }

        public void WaitForExit()
        {
            while (writethread.IsAlive || readthread.IsAlive)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("Terminated... {0}", this.client_tcp.GetHashCode());
        }


        void Read()
        {
            try
            {
                while (true)
                {
                    int read = server_Stream.ReadByte();
                    if (read == -1)
                        break;


                    Console.Write((char)read);
                    this.client_Stream.WriteByte((byte)read);
                }
            }
            catch (IOException)
            { }

            this.client_tcp.Close();
            this.server_tcp.Close();
            writethread.Abort();
        }


        Regex recipient_replace = new Regex("RCPT TO[ ]*:[ ]*<([^@]+@[^>]+)>[ ]*");
        Regex mail_from_replace = new Regex("MAIL FROM[ ]*:[ ]*<([^@]+@[^>]+)>[ ]*");
        Regex from_replace = new Regex("From:[ ]+.+");


        void Write()
        {
            StreamReader client_StreamReader = new StreamReader(client_Stream, Encoding.UTF7);

            try
            {
                do
                {

                    string Line = client_StreamReader.ReadLine();


                    if (recipient_replace.Match(Line).Success)
                    {
                        Line = recipient_replace.Replace(Line, string.Format("RCPT TO:<{0}>", this.settings.Recipient));
                    }
                    else if (mail_from_replace.Match(Line).Success)
                    {
                        Line = mail_from_replace.Replace(Line, string.Format("MAIL FROM:<{0}>", this.settings.Sender));
                    }
                    else if (from_replace.Match(Line).Success)
                    {
                        Line = from_replace.Replace(Line, string.Format("FROM: {0}", this.settings.Sender));
                    }


                    foreach (var c in Line.ToCharArray())
                    {
                        this.server_Stream.WriteByte((byte)c);
                    }
                    this.server_Stream.WriteByte(13);
                    this.server_Stream.WriteByte(10);
                    Console.WriteLine("> {0}", Line);



                } while (this.client_tcp.Connected);
            }
            catch (IOException)
            {
            }

            this.client_tcp.Close();
            this.server_tcp.Close();
            readthread.Abort();
        }
    }
}