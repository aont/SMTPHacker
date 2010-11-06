using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;

namespace MassMeasurement
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort serial = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            serial.ReadTimeout = 10000;
            serial.Open();



            char[] data = new char[serial.BaudRate];
            serial.WriteLine("IP");
            serial.Read(data, 0, serial.BaudRate);

            Regex regex = new Regex("MODE:	Weight\r\n(-?[0-9]+[.][0-9]+)[ ]+g([?]?)\r\nOK!", RegexOptions.Multiline);



            do
            {
                Thread.Sleep(100);
                serial.WriteLine("IP");
                serial.Read(data, 0, serial.BaudRate);


                var str = new string(data);
                var match = regex.Match(str);

                if (match.Success)
                {
                    if (match.Groups[2].Value == "")
                        Console.WriteLine("{0} g", match.Groups[1].Value);
                    else
                        Console.WriteLine("{0} g?", match.Groups[1].Value);
                }
                else
                    continue;
                //Console.WriteLine(str);
            } while (Console.ReadKey(true).KeyChar != 'x');

            serial.Close();
            serial.Dispose();

            //Console.ReadKey();
        }
    }
}