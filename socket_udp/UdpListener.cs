using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace socket_udp
{
    public class UDPSocket
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 40 * 1024;
        private State state = new State();
        //private IPEndPoint epTo; //server
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

            //epTo = new IPEndPoint(IPAddress.Parse(address), port);

            Console.WriteLine("Concluído.\n");
        }

        public void Client(string address, int port)
        {
            Console.WriteLine("Conectando ao server " + address + ":" + port + " ...");
            _socket.Connect(IPAddress.Parse(address), port);

            Receive();

            //epTo = new IPEndPoint(IPAddress.Parse(address), port);
        }

        public void Send(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);

                Console.WriteLine("Enviado: {0} bytes, conteúdo: {1}", bytes, text);

                //ClientHandler();
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

        //private void ClientHandler()
        //{
        //    Console.WriteLine("CLIENT MSG | Enviado: {0} bytes, conteúdo: {1}", bytes, text);
        //}

        private void ServerHandler(string bff)
        {
            string clientIp = ((IPEndPoint)epFrom).Address.ToString();
            //repository.CreateDirectory(clientIp);

            UDPSocket response = new UDPSocket();
            response.Client(clientIp, 29000);

            string[] request = bff.Split(';');
            if (request.Length > 0)
            {
                switch (request[0])
                {
                    case "PTA":
                        if (request.Length == 1)
                        {
                            response.Send($"ETA;{repository.GetDirectoryFiles()}");
                        }
                        //Send(repository.GetDirectoryList(clientIp));
                        break;
                    case "ETA":
                        if (request.Length == 2)
                        {
                            Console.WriteLine("Lista de arquivos recebidos de {0}: {1}", epFrom.ToString(), request[1]);

                            repository.CheckDirectoryUpdate(request[1], response);
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
                            repository.CreateFile(filename, file);
                        }
                        break;
                }
            }

            //Console.WriteLine("SERVER MSG | Recebido de {0}: {1}", epFrom.ToString(), bff);
        }
    }
}
