using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("CO2 UDP Server");

using (UdpClient socket = new UdpClient(5005))
{
    IPEndPoint clientEndPoint = null;

    while (true)
    {
        // Modtag data fra klienten
        byte[] received = socket.Receive(ref clientEndPoint);
        string receivedMessage = Encoding.UTF8.GetString(received);

        Console.WriteLine($"Received: {receivedMessage}");

        // Eksempel: Parse ppm fra beskeden
        if (receivedMessage.StartsWith("CO2 Level:"))
        {
            string ppmString = receivedMessage.Replace("CO2 Level:", "").Replace("ppm", "").Trim();
            if (int.TryParse(ppmString, out int co2Level))
            {
                // Lidt i tvivl hvilken kode skal være her,


            }
        }

        // Tjek for stopbesked
        if (receivedMessage.ToLower().Trim() == "stop")
        {
            Console.WriteLine("Stopping server...");
            break;
        }
    }
}

