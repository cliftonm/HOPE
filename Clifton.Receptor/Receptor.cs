/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor
{
	/// <summary>
	/// Event args for enabled state change.
	/// </summary>
	public class ReceptorEnabledEventArgs : EventArgs
	{
		public bool State { get; protected set; }

		public ReceptorEnabledEventArgs(bool state)
		{
			State = state;
		}
	}

	/// <summary>
	/// A Receptor is the container for a receptor instance (a separate assembly) and the assembly instance.
	/// </summary>
	public class Receptor : IReceptor
	{
		/// <summary>
		/// Fires when a receptor's enabled state changes.
		/// </summary>
		public event EventHandler<ReceptorEnabledEventArgs> EnabledStateChanged;

		protected bool enabled;

		/// <summary>
		/// The receptor name, determined either from the assembly filename or the name specified during construction.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// Returns the name of the assembly implementing the receptor.
		/// </summary>
		public string AssemblyName 
		{ 
			get { return assemblyName; }
			protected set { assemblyName = value; }
		}

		/// <summary>
		/// The instance of the receptor.  The assembly must declare one class implementing IReceptorInstance, and
		/// this is the class that is instantiated through reflection.
		/// </summary>
		public IReceptorInstance Instance { get; protected set; }

		/// <summary>
		/// False if the receptor instance has not yet been instantiated.  True if it has.
		/// </summary>
		public bool Instantiated { get; protected set; }

		/// <summary>
		/// When a receptor is disabled, it will not receive carriers.
		/// </summary>
		public bool Enabled 
		{
			get { return enabled; }
			set
			{
				enabled = value;
				EnabledStateChanged.Fire(this, new ReceptorEnabledEventArgs(enabled));
			}
		}

		protected string assemblyName;
		protected Assembly assembly;

		/// <summary>
		/// Internal constructor for factory method FromFile.
		/// </summary>
		protected Receptor(string name, Assembly assembly)
		{
			Name = name;
			this.assembly = assembly;
			Enabled = true;
		}

		/// <summary>
		/// Initializes a receptor given its name and assembly filename.
		/// Used, for example, to load a list of receptors into the application.
		/// </summary>
		public Receptor(string name, string assemblyName)
		{
			Name = name;
			this.assemblyName = assemblyName;
			Enabled = true;
		}

		/// <summary>
		/// For instances that the application defines and that implement IReceptorInstance,
		/// use this constructor to specify the existing instance.
		/// </summary>
		public Receptor(string name, IReceptorInstance inst)
		{
			Name = name;
			Instance = inst;
			Instantiated = true;
			Enabled = true;
		}

		/// <summary>
		/// Factory method for creating a receptor from a given assembly filename.
		/// </summary>
		public static Receptor FromFile(string filename)
		{
			string name = Path.GetFileNameWithoutExtension(filename).SplitCamelCase();
			Assembly assembly = Assembly.LoadFile(filename);

			Receptor r = new Receptor(name, assembly);
			r.AssemblyName = Path.GetFileName(filename);

			return r;
		}

		/// <summary>
		/// Load the assembly.
		/// </summary>
		public Receptor LoadAssembly()
		{
			string fullPath = ResolveFullPath(null, assemblyName);
			assembly = Assembly.LoadFile(fullPath);

			return this;
		}

		/// <summary>
		/// Instantiate the receptor from the pre-loaded assembly.
		/// The receptor system instance is passed to the constructor if the IReceptorInstance implementor.
		/// </summary>
		public Receptor Instantiate(IReceptorSystem rsys)
		{
			Type impType;

			try
			{
				impType = assembly.GetTypes().
						Where(t => t.IsClass).
						Where(c => c.GetInterfaces().Where(i => i.Name == "IReceptorInstance").Count() > 0).Single();
			}
			catch
			{
				throw new ApplicationException("Unable to locate the receptor assembly " + assemblyName);
			}

			try
			{
				Instance = Activator.CreateInstance(impType, new object[] { rsys }) as IReceptorInstance;
				Name = Instance.Name;
				Instance.Enabled = Enabled;
			}
			catch(Exception ex)	// Need to be able to see originating exception.
			{
				throw new ApplicationException("Unable to instantiate the receptor " + Name + "\r\n" + ex.Message);
			}

			Instantiated = true;

			return this;
		}

		/// <summary>
		/// Uses the executing assembly path if a manual path is not specified.
		/// Returns the fully qualified path.
		/// </summary>
		protected string ResolveFullPath(string manualPath, string assemblyName)
		{
			string fullPath;

			if (String.IsNullOrEmpty(manualPath))
			{
				string appLocation = Assembly.GetExecutingAssembly().Location.LeftOfRightmostOf('\\');
				fullPath = Path.Combine(appLocation, assemblyName);
			}
			else
			{
				fullPath = Path.Combine(manualPath, assemblyName);
			}

			return fullPath;
		}
	}
}
