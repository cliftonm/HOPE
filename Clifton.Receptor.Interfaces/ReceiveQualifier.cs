using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Receptor.Interfaces
{
	/// <summary>
	/// Associates a qualifier with a protocol of interest.
	/// </summary>
	public class ReceiveQualifier
	{
		public string Protocol { get; set; }
		public Func<dynamic, bool> Qualifier { get; set; }
		public Action<dynamic> Action { get; set; }

		/// <summary>
		/// Default constructor creates an always true qualifier.
		/// </summary>
		public ReceiveQualifier()
		{
			Qualifier = (_) => true;
			Action = (_) => { };
		}

		/// <summary>
		/// Protocol name only, creates an always true qualifier.
		/// </summary>
		public ReceiveQualifier(string protocol)
		{
			Protocol = protocol;
			Qualifier = (_) => true;
			Action = (_) => { };
		}

		/// <summary>
		/// Protocol name only, creates an always true qualifier with the specified action.
		/// </summary>
		public ReceiveQualifier(string protocol, Action<dynamic> action)
		{
			Protocol = protocol;
			Qualifier = (_) => true;
			Action = action;
		}

		/// <summary>
		/// Specify the protocol name and qualifier.
		/// </summary>
		public ReceiveQualifier(string protocol, Func<dynamic, bool> qualifier)
		{
			Protocol = protocol;
			Qualifier = qualifier;
			Action = (_) => { };
		}

		/// <summary>
		/// Specify the protocol name and qualifier and with the specified action.
		/// </summary>
		public ReceiveQualifier(string protocol, Func<dynamic, bool> qualifier, Action<dynamic> action)
		{
			Protocol = protocol;
			Qualifier = qualifier;
			Action = action;
		}
	}
}
