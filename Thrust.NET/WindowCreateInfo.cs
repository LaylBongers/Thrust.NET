using System.Drawing;

namespace Thrust
{
	public class WindowCreateInfo
	{
		public WindowCreateInfo()
		{
			// Default data
			Title = "Thrust.NET";
			HasFrame = true;
			Width = 800;
			Height = 600;
		}

		public string Title { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string IconPath { get; set; }
		public bool HasFrame { get; set; }
	}
}