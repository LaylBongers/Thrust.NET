using System.IO;
using Thrust;

namespace Example
{
	internal static class Program
	{
		private static void Main()
		{
			using (var shell = new ThrustShell(@"C:\thrust-shell\thrust_shell.exe"))
			{
				shell.DebugMode = true;

				var window = new Window(shell, new FileInfo("./index.html"));
				window.Show();
				window.Close += (s, e) => shell.StopEventLoop();

				shell.RunEventLoop();
			}
		}
	}
}