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
		public event EventHandler Blurred = (s, e) => { };
		public event EventHandler Focused = (s, e) => { };
		public event EventHandler Unresponsive = (s, e) => { };
		public event EventHandler Responsive = (s, e) => { };
		public event EventHandler WorkerCrashed = (s, e) => { };
		public event EventHandler<RemoteEventArgs> RemoteReceived = (s, e) => { };

		public void Show()
		{
			_shell.SendCommand("call", "show", null, _id, null, false).Forget();
		}

		public void SetFocus(bool focus)
		{
			_shell.SendCommand("call", "focus", null, _id, new JObject {{"focus", focus}}, false).Forget();
		}

		public void SetTitle(string title)
		{
			_shell.SendCommand("call", "set_title", null, _id, new JObject {{"title", title}}, false).Forget();
		}

		public void SetFullscreen(bool fullscreen)
		{
			_shell.SendCommand("call", "set_fullscreen", null, _id, new JObject {{"fullscreen", fullscreen}}, false).Forget();
		}

		public void SetKiosk(bool kiosk)
		{
			_shell.SendCommand("call", "set_kiosk", null, _id, new JObject {{"kiosk", kiosk}}, false).Forget();
		}

		public void Maximize()
		{
			_shell.SendCommand("call", "maximize", null, _id, null, false).Forget();
		}

		public void Minimize()
		{
			_shell.SendCommand("call", "minimize", null, _id, null, false).Forget();
		}

		public void Restore()
		{
			_shell.SendCommand("call", "restore", null, _id, null, false).Forget();
		}

		public void Close()
		{
			_shell.SendCommand("call", "close", null, _id, null, false).Forget();
		}

		public void OpenDevtools()
		{
			_shell.SendCommand("call", "open_devtools", null, _id, null, false).Forget();
		}

		public void CloseDevtools()
		{
			_shell.SendCommand("call", "close_devtools", null, _id, null, false).Forget();
		}

		public void Move(int x, int y)
		{
			_shell.SendCommand("call", "move", null, _id, new JObject {{"x", x}, {"y", y}}, false).Forget();
		}

		public void Resize(int width, int height)
		{
			_shell.SendCommand("call", "resize", null, _id, new JObject { { "width", width }, { "height", height } }, false).Forget();
		}

		public void SendRemote(JObject message)
		{
			_shell.SendCommand("call", "remote", null, _id, new JObject { { "message", message } }, false).Forget();
		}

		private void EventHandler(string type, JObject eventObj)
		{
			switch (type)
			{
				case "closed":
					Closed(this, EventArgs.Empty);
					break;
				case "blur":
					Blurred(this, EventArgs.Empty);
					break;
				case "focus":
					Focused(this, EventArgs.Empty);
					break;
				case "unresponsive":
					Unresponsive(this, EventArgs.Empty);
					break;
				case "responsive":
					Responsive(this, EventArgs.Empty);
					break;
				case "worker_crashed":
					WorkerCrashed(this, EventArgs.Empty);
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
				{"size", new JObject
				{
					{"width", createInfo.Width},
					{"height", createInfo.Height}
				}},
				{"title", createInfo.Title},
				{"icon_path", createInfo.IconPath},
				{"has_frame", createInfo.HasFrame}
			});

			return new Window(shell, (int) response["_target"]);
		}
	}
}