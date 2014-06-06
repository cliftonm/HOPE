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
	public class MembraneEventArgs : EventArgs
	{
		public IMembrane Membrane { get; protected set; }

		public MembraneEventArgs(IMembrane m)
		{
			Membrane = m;
		}
	}

	/// <summary>
	/// Membranes contain receptors and other membranes.
	/// </summary>
	public class Membrane : IMembrane
	{
		/// <summary>
		/// Fires when a new membrane is instantiated.
		/// </summary>
		public event EventHandler<MembraneEventArgs> NewMembrane;

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

		// Yuck.  Way to much conversion going on here.
		public ReadOnlyCollection<IReceptor> Receptors { get { return receptorSystem.Receptors.Cast<IReceptor>().ToList().AsReadOnly(); } }

		public IMembrane ParentMembrane { get; protected set; }

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

		public ICarrier CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal)
		{
			return receptorSystem.CreateCarrier(from, protocol, signal);
		}

		public void CreateCarrierIfReceiver(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal)
		{
			receptorSystem.CreateCarrierIfReceiver(from, protocol, signal);
		}

		public ICarrier CreateInternalCarrier(ISemanticTypeStruct protocol, dynamic signal)
		{
			return receptorSystem.CreateInternalCarrier(protocol, signal);
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

		public void Remove(IReceptor receptor)
		{
			receptorSystem.Remove(receptor);
		}

		public void Remove(IReceptorInstance receptorInstance)
		{
			receptorSystem.Remove(receptorInstance);
		}

		/// <summary>
		/// Recurse through the entire membrane tree to find the membranes containing the receptors in the list.
		/// </summary>
		public List<Membrane> GetMembranesContaining(List<IReceptor> receptorList)
		{
			List<Membrane> ret = new List<Membrane>();

			receptorList.ForEach(r => ret.Add(GetMembraneContaining(r)));

			// Return the distinct membranes.
			return ret.Distinct().ToList();
		}

		// TODO: Maintain a flat list to avoid this recursion.
		/// <summary>
		/// Recurse through membranes to find the membrane containing the desired receptor.
		/// </summary>
		public Membrane GetMembraneContaining(IReceptor searchFor)
		{
			Membrane ret = null;

			if (Receptors.Contains((Receptor)searchFor))
			{
				ret = this;
			}
			else
			{
				foreach (Membrane childMembrane in Membranes)
				{
					ret = childMembrane.GetMembraneContaining(searchFor);

					if (ret != null)
					{
						break;
					}
				}
			}

			return ret;
		}

		/// <summary>
		/// Moves receptors in this membrane to the target membrane.
		/// </summary>
		public void MoveReceptorsToMembrane(List<IReceptor> receptors, Membrane targetMembrane)
		{
			receptors.ForEach(r=>MoveReceptorToMembrane(r, targetMembrane));
		}

		/// <summary>
		/// Moves the receptor in this membrane to the specified target membrane.
		/// </summary>
		public void MoveReceptorToMembrane(IReceptor receptor, Membrane targetMembrane)
		{
			receptorSystem.Receptors.Remove((Receptor)receptor);
			targetMembrane.receptorSystem.Receptors.Add((Receptor)receptor);
		}

		public Membrane CreateInnerMembrane()
		{
			Membrane inner = new Membrane(SemanticTypeSystem);
			Membranes.Add(inner);
			inner.ParentMembrane = this;
			NewMembrane.Fire(this, new MembraneEventArgs(inner));

			return inner;
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
