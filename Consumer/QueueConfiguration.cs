namespace Consumer
{
	public class QueueConfiguration
	{
		public string ConnectionString { get; set; }
		public string QueueName { get; set; }
		public int MaxConcurrentCalls { get; set; }

		public QueueConfiguration()
		{
			MaxConcurrentCalls = 1;
		}
	}
}