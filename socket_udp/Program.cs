using System.Configuration;
using System.Threading;

namespace socket_udp
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverIP = IP.GetLocalIPAddress();
            string[] network_ips = ConfigurationManager.AppSettings["network_ips"].Split(',');
            int server_socket_port = int.Parse(ConfigurationManager.AppSettings["server_socket_port"]);
            int resend_time = int.Parse(ConfigurationManager.AppSettings["resend_time"]);

            //instancia o proprio server
            UDPSocket server = new UDPSocket();
            server.Server(serverIP, server_socket_port);

            UDPSocket client = new UDPSocket();
            while (true)
            {
                foreach (string ip in network_ips)
                {
                    client.Client(ip, server_socket_port);
                    client.GetFilesList();
                }

                Thread.Sleep(resend_time);
            }
        }
    }
}
