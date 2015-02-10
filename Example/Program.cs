using System.IO;
using Thrust;

namespace Example
{
	internal static class Program
	{
		private static void Main()
		{
			// IMPORTANT: Set any assets you want to use in your application to "Copy to Output Directory: Copy if newer"

			using (var shell = new ThrustShell(@"C:\thrust-shell\thrust_shell.exe"))
			{
				//shell.DebugMode = true;

				CreateIndexWindow(shell);

				shell.RunEventLoop();
			}
		}

		private static void CreateIndexWindow(ThrustShell shell)
		{
			// Create a new thrust window and show it on the screen
			var createInfo = new WindowCreateInfo
			{
				HasFrame = false
			};
			var window = new Window(shell, new FileInfo("./index.html"), createInfo);
			window.Show();
			//window.OpenDevtools();

			// If we close this window, we need to also stop thrust
			window.Closed += (s, e) => shell.StopEventLoop();
			window.RemoteReceived += (s, e) => window.Close();
		}
	}
}