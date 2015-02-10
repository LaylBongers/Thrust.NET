using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Thrust
{
	public sealed class Window
	{
		private readonly int _id;
		private readonly ThrustShell _shell;

		public Window(ThrustShell shell, FileInfo file, WindowCreateInfo createInfo = null)
			: this(shell, new Uri(file.FullName), createInfo)
		{
		}

		public Window(ThrustShell shell, Uri url, WindowCreateInfo createInfo = null)
		{
			if (createInfo == null)
			{
				createInfo = new WindowCreateInfo();
			}

			_shell = shell;
			_id = _shell.SendCommand("create", "", "window", null, new JObject
			{
				{"root_url", url.AbsoluteUri},
				{"title", createInfo.Title},
				{"has_frame", createInfo.HasFrame}
			});

			_shell.RegisterEventHandler(_id, EventHandler);
		}

		public event EventHandler Closed = (s, e) => { };
		public event EventHandler<RemoteEventArgs> RemoteReceived = (s, e) => { };

		public void Show()
		{
			_shell.SendCommand("call", "show", null, _id, null);
		}

		public void Close()
		{
			_shell.SendCommand("call", "close", null, _id, null);
		}

		public void OpenDevtools()
		{
			_shell.SendCommand("call", "open_devtools", null, _id, null);
		}

		private void EventHandler(string type, JObject eventObj)
		{
			switch (type)
			{
				case "closed":
					Closed(this, EventArgs.Empty);
					break;
				case "remote":
					RemoteReceived(this, new RemoteEventArgs
					{
						Message = (JObject) eventObj["message"]
					});
					break;
			}
		}
	}
}