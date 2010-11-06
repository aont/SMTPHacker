using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SMTPHacker
{

    class SMTPListener : IDisposable
    {

        List<SMTPRelayer> portforwards;
        TcpListener client_listener;
        SMTPSettings settings;
        public SMTPListener(SMTPSettings settings)
        {
            this.settings = settings;

            this.portforwards = new List<SMTPRelayer>();
            this.client_listener = new TcpListener(IPAddress.Loopback, this.settings.LocalPort);
        }

        public void Start()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(CreateInstance));
            thread.Start(null);
        }
        void Forwarding(object obj)
        {
            var pf = obj as SMTPRelayer;
            pf.WaitForExit();
            pf.Dispose();
        }
        void CreateInstance(object nullobj)
        {
            this.client_listener.Start();
            while (true)
            {
                try
                {
                    var pf = new SMTPRelayer(client_listener.AcceptTcpClient(), this.settings);
                    this.portforwards.Add(pf);
                    pf.Start();

                    Thread thread = new Thread(new ParameterizedThreadStart(Forwarding));
                    thread.Start(pf);

                }
                catch (SocketException ex)
                {
                    // Server Stop Call
                    if (ex.ErrorCode == 10004)
                        break;
                    else
                        throw;
                }
            }
        }

        public void Dispose()
        {
            foreach (var pf in this.portforwards.ToArray())
            {
                pf.Dispose();
            }
            this.client_listener.Stop();
        }
    }
}