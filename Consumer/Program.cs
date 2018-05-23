using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Consumer
{
	class Program
	{
		private static QueueClient _queueClient;
		private static AppSettings _appSettings;

        private static readonly AutoResetEvent _exitEvent = new AutoResetEvent(false);
		static void Main(string[] args)
		{
			Console.CancelKeyPress += (o, e) =>
			{
				Console.WriteLine($"{DateTime.Now}: Exiting...");
				_exitEvent.Set();
			};

			Task.Run(() =>
			{
				var configuration = new ConfigurationBuilder()
					.AddJsonFile("Config/appsettings.json")
					.Build();

				_appSettings = configuration.Get<AppSettings>();

				_queueClient = new QueueClient(_appSettings.QueueConfiguration.ConnectionString, _appSettings.QueueConfiguration.QueueName);

				RegisterMessageHandler();
			});

			_exitEvent.WaitOne();

			_queueClient.CloseAsync().GetAwaiter().GetResult();
		}

		private static void RegisterMessageHandler()
		{
			var options = new MessageHandlerOptions(ExceptionReceivedHandler)
			{
				MaxConcurrentCalls = _appSettings.QueueConfiguration.MaxConcurrentCalls,
				AutoComplete = false
			};

			_queueClient.RegisterMessageHandler(ProcessMessagesAsync, options);

			Console.WriteLine($"{DateTime.Now}: Listening for messages...");
		}


		private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
		{
			Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
			var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
			Console.WriteLine("Exception context for troubleshooting:");
			Console.WriteLine($"- Endpoint: {context.Endpoint}");
			Console.WriteLine($"- Entity Path: {context.EntityPath}");
			Console.WriteLine($"- Executing Action: {context.Action}");
			return Task.CompletedTask;
		}

		private static async Task ProcessMessagesAsync(Message message, CancellationToken cancellationToken)
		{
			var messageBody = Encoding.UTF8.GetString(message.Body);

			Console.WriteLine($"{DateTime.Now}: Received message {message.SystemProperties.SequenceNumber}: {messageBody}");

			await Task.Delay(_appSettings.SimulatedWorkDuration, cancellationToken);

			await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
		}
	}
}
