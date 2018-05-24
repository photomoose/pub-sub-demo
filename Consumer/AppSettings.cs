using System;

namespace Consumer
{
	public class AppSettings
	{
		public int SimulatedWorkDuration { get; set; }
		public QueueConfiguration QueueConfiguration { get; set; }
		public Uri HubUri { get; set; }
	}
}