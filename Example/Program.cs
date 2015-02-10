using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Thrust;

namespace Example
{
	internal static class Program
	{
		private static void Main()
		{
			// IMPORTANT: Set any assets you want to use in your application to "Copy to Output Directory: Copy if newer";
			AsyncMain().Wait();
		}

		private static async Task AsyncMain()
		{
			using (var shell = new ThrustShell(@"C:\thrust-shell\thrust_shell.exe"))
			{
				//shell.DebugMode = true;
				shell.StartEventLoop();

				await CreateIndexWindow(shell);

				shell.AwaitStop();
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
				switch ((string) e.Message["action"])
				{
					case "close":
						window.Close();
						break;
					case "button_click":
						window.SendRemote(new JObject {{"Hello", "From Thrust.NET!"}});
						break;
				}
			};
		}
	}
}