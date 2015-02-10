using System;
using Newtonsoft.Json.Linq;

namespace Thrust
{
	public sealed class RemoteEventArgs : EventArgs
	{
		public JObject Message { get; set; }
	}
}