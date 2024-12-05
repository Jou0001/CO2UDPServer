using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CO2DatabaseLib;
using CO2DatabaseLib.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using System.Net.Mail;

class Program
{
	static async Task Main(string[] args)
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

					// Split beskeden i SensorId og CO2Value
					string[] splitMessage = message.Split(' ');

					if (splitMessage.Length == 2 &&
						int.TryParse(splitMessage[0], out int sensorId) &&
						int.TryParse(splitMessage[1], out int co2Value))
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

						if (co2Value >= 1000)
						{
							Console.WriteLine("Critical CO2 level detected! Sending email...");
							await SendEmail(sensorId, co2Value);
						}
					}
					else
					{
						Console.WriteLine($"Invalid data format: {message}. Expected format: 'SensorId CO2Value'");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"An error occurred: {ex.Message}");
				}
			}
		}
	}

	static async Task SendEmail(int sensorId, int co2Value)
	{
		try
		{
			var apiKey = "din_sendgrid_api_key";
			var client = new SendGridClient(apiKey);
			var from = new EmailAddress("din.email@domain.com", "CO2 Monitoring System");
			var subject = "Critical CO2 Level Alert!";
			var to = new EmailAddress("modtagerens.email@domain.com", "Modtager Navn");
			var plainTextContent = $"Alert! Sensor {sensorId} detected a CO2 level of {co2Value} ppm, which is above the critical threshold.";
			var htmlContent = $"<strong>Alert!</strong> Sensor {sensorId} detected a CO2 level of {co2Value} ppm.";
			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

			var response = await client.SendEmailAsync(msg);
			Console.WriteLine($"Email sent! Status Code: {response.StatusCode}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to send email: {ex.Message}");
		}
	}
}
