using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Publisher
{
    class Program
    {
	    private static AppSettings _appSettings;

	    static void Main(string[] args)
        {
	        MainAsync(args).GetAwaiter().GetResult();
        }

	    static async Task MainAsync(string[] args)
	    {
		    var configuration = new ConfigurationBuilder()
			    .AddJsonFile("Config/appsettings.json")
			    .Build();

		    _appSettings = configuration.Get<AppSettings>();

			var queueClient = new QueueClient(_appSettings.QueueConfiguration.ConnectionString, _appSettings.QueueConfiguration.QueueName);

		    var messageNumber = 1;

		    while (true)
		    {
			    var messageBody = $"Message #{messageNumber++}";
			    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

			    Console.WriteLine($"{DateTime.Now}: Sending message: {messageBody}");

			    try
			    {
				    await queueClient.SendAsync(message);
			    }
			    catch (Exception ex)
			    {
				    Console.WriteLine($"{DateTime.Now}: {ex.Message}");
			    }

			    await Task.Delay(_appSettings.DispatchFrequency);
		    }
	    }
    }
}
