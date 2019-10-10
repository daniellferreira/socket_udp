using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace socket_udp
{
    public class UDPSocket
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 16 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0); //client
        private AsyncCallback recv = null;
        private Repository repository = new Repository();

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string address, int port)
        {
            Console.WriteLine("Iniciando o server => " + address + ":" + port + " ...");
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));

            Receive();
            repository.IniRepository(address);

            Console.WriteLine("Concluído.\n");
        }

        public void Client(string address, int port)
        {
            Console.WriteLine("\nConectando ao server " + address + ":" + port + " ...");
            _socket.Connect(IPAddress.Parse(address), port);

            Receive();
        }

        public void Send(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);

                ClientHandler(text);
            }, state);
        }

        private void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);

                ServerHandler(Encoding.UTF8.GetString(so.buffer, 0, bytes));
            }, state);
        }

        private void ClientHandler(string text)
        {
            Console.WriteLine("Enviado: {0}", text);
        }

        private void ServerHandler(string bff)
        {
            string clientIp = ((IPEndPoint)epFrom).Address.ToString();

            Console.WriteLine("Recebido de {0}: {1}", clientIp, bff);

            UDPSocket response = new UDPSocket();
            response.Client(clientIp, 29000);
            string[] request = bff.Split(';');

            if (request.Length > 0)
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    switch (request[0])
                    {
                        case "PTA":
                            if (request.Length == 1)
                            {
                                response.Send($"ETA;{repository.GetDirectoryFiles()}");
                            }
                            break;
                        case "ETA":
                            if (request.Length == 2)
                            {
                                foreach (string requestFile in repository.CheckDirectoryUpdate(request[1], response, clientIp))
                                {
                                    //se houver necessidade pede os arquivos      
                                    response.Send($"PAE;{requestFile}");
                                    Thread.Sleep(50);
                                }
                            }
                            break;
                        case "PAE":
                            if (request.Length == 2)
                            {
                                string filename = request[1];
                                byte[] file = repository.GetFile(filename);

                                response.Send($"EAE;{file.Length};{filename};{Encoding.UTF8.GetString(file, 0, file.Length)}");
                            }
                            break;
                        case "EAE":
                            if (request.Length == 4)
                            {
                                string filename = request[2];
                                byte[] file = Encoding.UTF8.GetBytes(request[3]);
                                repository.CreateFile(clientIp, filename, file);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                } finally
                {
                    watch.Stop();

                    Console.WriteLine("Tempo de processamento [{0}]: {1}ms", request[0], watch.Elapsed.TotalMilliseconds);
                }
            }
        }

        public void GetFilesList()
        {
            Send("PTA");
        }
    }
}
