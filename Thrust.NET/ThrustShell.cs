using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thrust
{
	public sealed class ThrustShell : IDisposable
	{
		private const string Boundary = "--(Foo)++__THRUST_SHELL_BOUNDARY__++(Bar)--";
		private readonly Dictionary<int, JObject> _commandResults = new Dictionary<int, JObject>();
		private readonly Dictionary<int, EventWaitHandle> _commandWaitHandles = new Dictionary<int, EventWaitHandle>();

		private readonly Dictionary<int, Action<string, JObject>> _eventHandlers =
			new Dictionary<int, Action<string, JObject>>();

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

				switch ((string) evt["_action"])
				{
					case "event":
						// Get data from the event
						var target = (int) evt["_target"];
						var type = (string) evt["_type"];
						var eventObj = (JObject) evt["_event"];

						// Pass on the event to the appropriate handler
						_eventHandlers[target].Invoke(type, eventObj);
						break;
					case "reply":
						// Get data from the response
						var result = (JObject) evt["_result"];
						var commandId = (int) evt["_id"];

						// Check if we have an awaiting command
						if (_commandWaitHandles.ContainsKey(commandId))
						{
							// Write the data to the result dictionary
							_commandResults[commandId] = result;

							// Signal the waiting command we're done
							_commandWaitHandles[commandId].Set();
						}

						break;
				}
			}
		}

		public void StopEventLoop()
		{
			_keepRunning = false;
		}

		internal void RegisterEventHandler(int targetId, Action<string, JObject> action)
		{
			_eventHandlers[targetId] = action;
		}

		internal async Task<JObject> SendCommand(string action, string method, string type, int? target, JObject arguments, bool waitForResponse = true)
		{
			var commandId = GetNextId();
			var jsonCommand = new JObject
			{
				// Used to reference back whatever we're about to do at a later point
				{"_id", commandId},
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

			if (waitForResponse)
			{
				// Create a new wait handle for our command so we can wait for a response
				var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
				_commandWaitHandles[commandId] = waitHandle;

				// Actually send over the command
				WriteJson(jsonCommand);

				// Hack to be able to wait on WaitOne()
				return await Task.Run(() =>
				{
					// Wait till the event loop thread signals us
					waitHandle.WaitOne();

					// Retrieve the data the event loop has for us
					var result = _commandResults[commandId];
					_commandResults.Remove(commandId);

					return result;
				});
			}
			else
			{
				// Actually send over the command
				WriteJson(jsonCommand);

				return null;
			}
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