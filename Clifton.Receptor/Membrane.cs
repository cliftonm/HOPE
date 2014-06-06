using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor
{
	/// <summary>
	/// Membranes contain receptors and other membranes.
	/// </summary>
	public class Membrane
	{
		/// <summary>
		/// Fires when a new receptor is instantiated and registered into the system.
		/// </summary>
		public event EventHandler<ReceptorEventArgs> NewReceptor;

		/// <summary>
		/// Fires when a new carrier is created.
		/// </summary>
		public event EventHandler<NewCarrierEventArgs> NewCarrier;

		/// <summary>
		/// Fires when a receptor is removed.
		/// </summary>
		public event EventHandler<ReceptorEventArgs> ReceptorRemoved;

		/// <summary>
		/// A membrane has permeability to protocols.
		/// </summary>
		public enum Permeability
		{
			None,
			In,
			Out,
			Bidirectional,
		}

		public string Name { get; set; }
		public ISemanticTypeSystem SemanticTypeSystem { get; protected set; }
		public List<Membrane> Membranes { get; protected set; }
		public Dictionary<string, Permeability> ProtocolPermeability {get; protected set;}
		public ReadOnlyCollection<Receptor> Receptors { get { return receptorSystem.Receptors.AsReadOnly(); } }

		protected ReceptorsContainer receptorSystem;
		protected ISemanticTypeSystem semanticTypeSystem;

		public Membrane(ISemanticTypeSystem sts)
		{
			receptorSystem = new ReceptorsContainer(sts);
			SemanticTypeSystem = sts;

			receptorSystem.NewReceptor += OnNewReceptor;
			receptorSystem.NewCarrier += OnNewCarrier;
			receptorSystem.ReceptorRemoved += OnReceptorRemoved;

			Membranes = new List<Membrane>();
			ProtocolPermeability = new Dictionary<string, Permeability>();
		}

		/// <summary>
		/// Resets the receptor system, clears the protocol permeability, recursively resets inner membranes, and dissociates all child receptors.
		/// </summary>
		public void Reset()
		{
			Membranes.ForEach(m => m.Reset());
			receptorSystem.Reset();
			Membranes.Clear();
			ProtocolPermeability.Clear();
		}

		public void LoadReceptors(Action<Receptor> afterRegister = null)
		{
			receptorSystem.LoadReceptors(afterRegister);
		}

		public void CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal)
		{
			receptorSystem.CreateCarrier(from, protocol, signal);
		}

		public void RegisterReceptor(string fn)
		{
			receptorSystem.RegisterReceptor(fn);
		}

		public IReceptor RegisterReceptor(string name, string assemblyName)
		{
			IReceptor receptor = receptorSystem.RegisterReceptor(name, assemblyName);

			return receptor;
		}

		public void RegisterReceptor(string name, IReceptorInstance inst)
		{
			receptorSystem.RegisterReceptor(name, inst);
		}

		public void RemoveReceptor(IReceptor receptor)
		{
			receptorSystem.Remove(receptor);
		}

		protected void OnNewReceptor(object sender, ReceptorEventArgs args)
		{
			// Forward to other listeners.
			NewReceptor.Fire(this, args);
		}

		protected void OnNewCarrier(object sender, NewCarrierEventArgs args)
		{
			// Forward to other listeners.
			NewCarrier.Fire(this, args);
		}

		protected void OnReceptorRemoved(object sender, ReceptorEventArgs args)
		{
			// Forward to other listeners.
			ReceptorRemoved.Fire(this, args);
		}
	}
}
