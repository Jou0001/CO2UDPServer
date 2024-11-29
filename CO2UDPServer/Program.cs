using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

class Program
{
	// Liste til at gemme CO2-data
	private static List<string> co2Data = new List<string>();

	static void Main(string[] args)
	{
		Console.WriteLine("Starting UDP server...");

		using (UdpClient socket = new UdpClient())
		{
			// Bind serveren til port 5005
			socket.Client.Bind(new IPEndPoint(IPAddress.Any, 5005));
			Console.WriteLine("Server is listening on port 5005...");

			while (true)
			{
				// Modtag data fra klienten
				IPEndPoint clientEndpoint = null;
				byte[] receivedData = socket.Receive(ref clientEndpoint);

				// Konverter data til string
				string message = Encoding.UTF8.GetString(receivedData);

				// Gem data i listen og udskriv
				co2Data.Add(message);
				Console.WriteLine($"Received CO2 data: {message} ppm from {clientEndpoint.Address}:{clientEndpoint.Port}");

				// Vis alle gemte målinger
				Console.WriteLine("Stored measurements:");
				foreach (var data in co2Data)
				{
					Console.WriteLine(data);
				}

				if (int.TryParse(message, out int co2Value) && co2Value >= 1000)
				{
					Console.WriteLine("ALERT: Critical CO2 level detected!");
				}

			}
		}
	}
}
