using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Thrust
{
	public sealed class Window
	{
		private readonly int _id;
		private readonly ThrustShell _shell;

		public Window(ThrustShell shell, FileInfo file)
			:this(shell, new Uri(file.FullName))
		{
		}

		public Window(ThrustShell shell, Uri url)
		{
			_shell = shell;
			_id = _shell.SendCommand("create", "", "window", null, new JObject
			{
				{"root_url", url.AbsoluteUri},
				{"title", "Thrust.NET"}
			});

			_shell.RegisterEventHandler(_id, EventHandler);
		}

		public event EventHandler Close = (s, e) => { };

		public void Show()
		{
			_shell.SendCommand("call", "show", null, _id, null);
		}

		private void EventHandler(string type)
		{
			switch (type)
			{
				case "closed":
					Close(this, EventArgs.Empty);
					break;
			}
		}
	}
}