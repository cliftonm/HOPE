using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace Clifton.Receptor
{
	public class Receptor : IReceptor
	{
		public string Name { get; protected set; }
		public IReceptorInstance Instance { get; protected set; }
		public bool Processed { get; set; }

		protected string assemblyName;
		protected Assembly assembly;

		public Receptor()
		{
		}

		public Receptor(string name, string assemblyName)
		{
			Name = name;
			this.assemblyName = assemblyName;
		}

		/// <summary>
		/// For internal receptors.
		/// </summary>
		public Receptor(string name, IReceptorInstance inst)
		{
			Name = name;
			Instance = inst;
		}

		public void FromFile(string filename)
		{
			Name = Path.GetFileNameWithoutExtension(filename).SplitCamelCase();
			assembly = Assembly.LoadFile(filename);
		}

		public void LoadAssembly()
		{
			string fullPath = ResolveFullPath(null, assemblyName);
			assembly = Assembly.LoadFile(fullPath);
		}

		public void Instantiate(IReceptorSystem rsys)
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
			}
			catch
			{
				throw new ApplicationException("Unable to instantiate the receptor " + Name);
			}
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
