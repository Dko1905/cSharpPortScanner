using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

namespace PortScannerConsole
{
    class Program
    {
        private static Queue results = Queue.Synchronized(new Queue());
        private static TimeSpan timeout;
        private static string getIp()
        {
            string ipAsString;
            Console.WriteLine("Please write 1 ip:");
            ipAsString = Console.ReadLine();
            return ipAsString;
        }
        private static Int32[] getPorts()
        {
            Int32 startPort, endPort;
            Int32[] ports;
            Console.WriteLine("Please write 2 ports that it will scan between in two lines:");
            startPort = Convert.ToInt32(Console.ReadLine());
            endPort = Convert.ToInt32(Console.ReadLine());
            ports = new int[endPort - startPort + 1];

            for(Int32 currentPort = 0; currentPort < endPort - startPort + 1; currentPort++)
            {
                ports[currentPort] = currentPort + startPort;
            }
            return ports;
        }
        static void setTimeout()
        {
            Console.WriteLine("Please write the timeout in seconds");
            int timeoutInS = Convert.ToInt32(Console.ReadLine());
            timeout = new TimeSpan(0, 0, timeoutInS);
            Console.WriteLine($"Sat timeout to {timeout.TotalSeconds} seconds!");
        }
        static void IsPortOpen(object ipAndPort)
        {
            string ip = ((Tuple<string, Int32>)ipAndPort).Item1;
            int port = ((Tuple<string, Int32>)ipAndPort).Item2;
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(ip, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        //results.Enqueue(Tuple.Create<string, int, bool>(ip, port, false));
                        return;
                    }
                    client.EndConnect(result);
                    client.Close();
                }
            }
            catch
            {
                //results.Enqueue(Tuple.Create<string, int, bool>(ip, port, false));
                return;
            }
            results.Enqueue(Tuple.Create<string, int, bool>(ip, port, true));
            return;
        }

        static void giveOutPortsThread(string ip, Int32[] ports)
        {
            Console.WriteLine("Starting threads!");
            for (int i = 0; i < ports.Length - 1; i++)
            {
                new Thread(new ParameterizedThreadStart(IsPortOpen)).Start(Tuple.Create<string, int>(ip, ports[i]));
            }
        }
        static void giveOutPortsSingle(string ip, Int32[] ports) 
        {
            Console.WriteLine("Running tasks!");
            for (int i = 0; i < ports.Length; i++)
            {
                IsPortOpen( Tuple.Create<string, int>(ip, ports[i]) );
            }
        }
        static void readIPsConst()
        {
            while (true)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    Tuple<string, Int32, bool> t = (Tuple<string, Int32, bool>)results.Dequeue();
                    Console.WriteLine($"{t.Item1} : {t.Item2} ; {t.Item3}");
                }
                Thread.Sleep(250);
            }
        }
        static void Main(string[] args)
        {
            string ip = getIp();
            Int32[] ports = getPorts();
            setTimeout();
            new Thread(new ThreadStart(readIPsConst)).Start();
            giveOutPortsThread(ip, ports);
            
            Console.Read();
        }
    }
}
