using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace WpfUdpChat
{
    internal static class UdpProtocol
    {
        static public void Send(UdpClient client,string host,int remotePort,string message)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, host, remotePort);
            }
            catch (Exception)
            {

            }
        }
        static public void Exit(UdpClient client,string host)
        {
            client.DropMulticastGroup(IPAddress.Parse(host));
            client.Close();
        }
    }
}
