using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CO2DatabaseLib;
using CO2DatabaseLib.Models;

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Starting the UDP server...");

		using (UdpClient socket = new UdpClient(5005)) 
		{
			socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			Console.WriteLine("Listening on port 5005...");


			var db = new DBConnection();

			while (true)
			{
				try
				{
					IPEndPoint clientEndpoint = null;
					byte[] receiveData = socket.Receive(ref clientEndpoint);


					string message = Encoding.UTF8.GetString(receiveData);
					Console.WriteLine($"Received message: {message} from {clientEndpoint.Address}:{clientEndpoint.Port}");

					string[] splitMessage = message.Split(' ');

					if (int.TryParse(splitMessage[0], out int sensorId) && int.TryParse(splitMessage[1], out int co2Value))
					{
						Console.WriteLine($"Parsed SensorId: {sensorId} & CO2 value: {co2Value} ppm");


						var measurement = new Measurement
						{
							SensorId = sensorId,
							MeasurementTime = DateTime.Now,
							MeasurementValue = co2Value
						};

						db._dbContext.Measurements.Add(measurement);
						db._dbContext.SaveChanges();

						Console.WriteLine("Measurement saved to database.");
					}
					else
					{
						Console.WriteLine($"Invalid data received: {message}");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"An error occurred: {ex.Message}");
				}
			}
		}
	}
}
