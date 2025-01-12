﻿using System;
using System.Net;
using System.Net.Sockets;

namespace socket_udp
{
    class IP
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Não há um endereço IP disponível");
        }
    }
}
