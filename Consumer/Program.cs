using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Consumer
{
	class Program
	{
		private static QueueClient _queueClient;
		private static AppSettings _appSettings;

        private static readonly AutoResetEvent _exitEvent = new AutoResetEvent(false);
		private static HubConnection _hubConnection;

		static void Main(string[] args)
		{
			Console.CancelKeyPress += (o, e) =>
			{
				Console.WriteLine($"{DateTime.Now}: Exiting...");
				_exitEvent.Set();
			};

			Task.Run(async () =>
			{
				_appSettings = new ConfigurationBuilder()
					.AddJsonFile("Config/appsettings.json")
					.Build()
					.Get<AppSettings>();

				_hubConnection = new HubConnectionBuilder()
					.WithUrl(_appSettings.HubUri)
					.Build();

				var isConnected = false;

				while (!isConnected)
				{

					try
					{
						Console.Write($"Connecting to hub {_appSettings.HubUri}...");
						await _hubConnection.StartAsync();
						Console.WriteLine("OK");
						isConnected = true;
					}
					catch (HttpRequestException ex)
					{
						Console.WriteLine("FAILED");
						await Task.Delay(5000);
					}
				}

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

			Console.Write($"{DateTime.Now}: Sending message {message.SystemProperties.SequenceNumber} to hub...");
			await _hubConnection.InvokeAsync("SendMessage", Environment.MachineName, messageBody, cancellationToken);
			Console.WriteLine("OK");

			await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
		}
	}
}
