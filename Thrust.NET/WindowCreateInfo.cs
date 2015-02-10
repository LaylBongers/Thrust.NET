namespace Thrust
{
	public class WindowCreateInfo
	{
		public WindowCreateInfo()
		{
			// Default data
			Title = "Thrust.NET";
			HasFrame = true;
		}

		public string Title { get; set; }
		public bool HasFrame { get; set; }
	}
}