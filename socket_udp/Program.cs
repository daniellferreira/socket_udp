using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace socket_udp
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverIP = IP.GetLocalIPAddress();

            //instancia o server na porta 29000
            UDPSocket server = new UDPSocket();
            try
            {
                server.Server(serverIP, 29000);
            }
            catch (SocketException ex)
            {
                //ja existe um server neste IP com a porta 29000
                //Console.WriteLine("Já existe um server para " + serverIP + ":" + 29000);
                //Console.WriteLine("Será iniciado o server local");
                //server.Server("127.0.0.1", 29000);
            }


            //instancia o client chamando o server que está na porta 29000
            //client.servesToSend.Add(client);

            //UDPSocket clientItem = new UDPSocket();
            //clientItem.Client("127.0.0.1", 29000);
            //server.clients.Add(clientItem);


            UDPSocket client = new UDPSocket();
            Console.WriteLine(string.Empty);

            //AQUI FAZER O LOOP COM SLEEP PARA CHAMAR A VERIFICACAO DE DIRETORIO

            string[] network_ips = ConfigurationManager.AppSettings["network_ips"].Split(',');
            int server_socket_port = int.Parse(ConfigurationManager.AppSettings["server_socket_port"]);

            try
            {
                foreach (string ip in network_ips)
                {
                    client.Client(ip, server_socket_port);
                    string msg = string.Empty;
                    while ((msg = Console.ReadLine()) != "exit")
                    {
                        client.Send("PTA");
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            while (true) { }
        }
    }
}
