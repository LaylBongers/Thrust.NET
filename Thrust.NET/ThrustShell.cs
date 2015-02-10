using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thrust
{
	public sealed class ThrustShell : IDisposable
	{
		private const string Boundary = "--(Foo)++__THRUST_SHELL_BOUNDARY__++(Bar)--";
		private readonly Dictionary<int, Action<string>> _eventHandlers = new Dictionary<int, Action<string>>();
		private readonly Process _process;
		private bool _keepRunning;
		private int _lastId;

		public ThrustShell(string shellPath)
		{
			var startInfo = new ProcessStartInfo(shellPath)
			{
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			_process = Process.Start(startInfo);
		}

		/// <summary>
		///     Gets or sets if debug information will be written to the console.
		/// </summary>
		public bool DebugMode { get; set; }

		public void Dispose()
		{
			// There's probably a better way to stop Thrust but right now this works
			// TODO: Find that better way
			_process.Kill();
			_process.Dispose();
		}

		public void RunEventLoop()
		{
			if (_keepRunning)
			{
				throw new InvalidOperationException("Cannot start running callback loop if already running.");
			}

			// Run the actual event loop
			_keepRunning = true;
			while (_keepRunning)
			{
				var evt = ReadJson();

				// Write to the console for debugging
				if (DebugMode)
				{
					Console.WriteLine(evt.ToString(Formatting.None));
				}

				// Make sure it's an event, we're only interested in that
				// TODO: Handle responses as well and make window creation async using them
				if ((string) evt["_action"] != "event")
					continue;

				// Get data from the event
				var target = (int) evt["_target"];
				var type = (string) evt["_type"];

				// Pass on the event to the appropriate handler
				_eventHandlers[target].Invoke(type);
			}
		}

		public void StopEventLoop()
		{
			_keepRunning = false;
		}

		internal void RegisterEventHandler(int targetId, Action<string> action)
		{
			_eventHandlers[targetId] = action;
		}

		internal int SendCommand(string action, string method, string type, int? target, JObject arguments)
		{
			var id = GetNextId();
			var jsonCommand = new JObject
			{
				// Used to reference back whatever we're about to do at a later point
				{"_id", id},
				// Communicates if we want to create or call
				{"_action", action},
				// Communicates what we want to call if applicable 
				{"_method", method},
				// Communicates what type we want to create if applicable
				{"_type", type},
				// Communicates what we want to call on if applicable
				{"_target", target},
				// Arguments for our creation or call
				{"_args", arguments}
			};

			// Actually send over the command
			WriteJson(jsonCommand);

			return id;
		}

		private void WriteJson(JObject jsonCommand)
		{
			_process.StandardInput.Write(jsonCommand.ToString(Formatting.None) + "\n" + Boundary + "\n");
			_process.StandardInput.Flush();
		}

		private JObject ReadJson()
		{
			string raw;

			// Wait till we get something that isn't the boundary
			while ((raw = _process.StandardOutput.ReadLine()) == Boundary)
			{
			}

			// We got a json object, parse it!
			return JObject.Parse(raw);
		}

		private int GetNextId()
		{
			return Interlocked.Increment(ref _lastId);
		}
	}
}