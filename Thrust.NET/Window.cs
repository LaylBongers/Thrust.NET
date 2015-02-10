using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Thrust
{
	public sealed class Window
	{
		private readonly int _id;
		private readonly ThrustShell _shell;

		private Window(ThrustShell shell, int id)
		{
			_shell = shell;
			_id = id;

			_shell.RegisterEventHandler(_id, EventHandler);
		}

		public event EventHandler Closed = (s, e) => { };
		public event EventHandler<RemoteEventArgs> RemoteReceived = (s, e) => { };

		public void Show()
		{
			_shell.SendCommand("call", "show", null, _id, null, false).Forget();
		}

		public void Close()
		{
			_shell.SendCommand("call", "close", null, _id, null, false).Forget();
		}

		public void SendRemote(JObject message)
		{
			_shell.SendCommand("call", "remote", null, _id, new JObject {{"message", message}}, false).Forget();
		}

		public void OpenDevtools()
		{
			_shell.SendCommand("call", "open_devtools", null, _id, null, false).Forget();
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

		public static async Task<Window> Create(ThrustShell shell, FileInfo fileInfo, WindowCreateInfo createInfo = null)
		{
			return await Create(shell, new Uri(fileInfo.FullName), createInfo);
		}

		public static async Task<Window> Create(ThrustShell shell, Uri uri, WindowCreateInfo createInfo = null)
		{
			if (createInfo == null)
			{
				createInfo = new WindowCreateInfo();
			}

			var response = await shell.SendCommand("create", "", "window", null, new JObject
			{
				{"root_url", uri.AbsoluteUri},
				{"title", createInfo.Title},
				{"has_frame", createInfo.HasFrame}
			});

			return new Window(shell, (int) response["_target"]);
		}
	}
}