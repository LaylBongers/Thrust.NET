using System.IO;
using System.Threading.Tasks;
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

				CreateIndexWindow(shell).Forget();

				shell.RunEventLoop();
			}
		}

		private static async Task CreateIndexWindow(ThrustShell shell)
		{
			// Create a new thrust window and show it on the screen
			var createInfo = new WindowCreateInfo
			{
				HasFrame = false
			};
			var window = await Window.Create(shell, new FileInfo("./index.html"), createInfo);
			window.Show();
			//window.OpenDevtools();

			// If we close this window, we need to also stop thrust
			window.Closed += (s, e) => shell.StopEventLoop();
			window.RemoteReceived += (s, e) =>
			{
				if ((string) e.Message["payload"] == "close")
				{
					window.Close();
				}
			};
		}
	}
}