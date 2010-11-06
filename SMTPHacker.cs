using System;
using System.IO;

namespace SMTPHacker
{
    class Program
    {
        static void Main(string[] args)
        {
            SMTPSettings settings;
            FileInfo defaultsettings = new FileInfo("settings.cfg");
            if (defaultsettings.Exists)
            {
                settings = SMTPSettings.Load(defaultsettings.FullName);

                var smtplistener = new SMTPListener(settings);
                smtplistener.Start();

                Console.WriteLine("Started...");
                Console.ReadKey();

                smtplistener.Dispose();
            }
            else
            {
                Console.WriteLine("Created settings.cfg.");
                Console.WriteLine("Edit this file to customize.");
                settings = new SMTPSettings()
                {
                    LocalPort = 25,
                    Recipient ="recipients@foo.com",
                    RemoteAddress = "smtp.com",
                    RemotePort = 25,
                    Sender = "sender@foo.com"
                };
                settings.Save(defaultsettings.FullName);
                return;
            }

        }
    }





}