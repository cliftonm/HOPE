using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor.Interfaces
{
	/// <summary>
	/// All receptors must implement this interface in order to be dynamically loaded at runtime.
	/// </summary>
	public interface IReceptorInstance
	{
		string Name { get; }
		bool IsEdgeReceptor { get; }
		bool IsHidden { get; }

		// The receptor system must be reset when a receptor moves to a different membrane,
		// that is, to another receptor system.
		IReceptorSystem ReceptorSystem { get; set; }

		string[] GetReceiveProtocols();
		string[] GetEmittedProtocols();
		void ProcessCarrier(ICarrier carrier);

		/// <summary>
		/// Called when the receptor initialization has completed.
		/// </summary>
		void Initialize();

		void Terminate();
	}

	public interface IReceptorSystem
	{
		ISemanticTypeSystem SemanticTypeSystem { get; }
		ICarrier CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal);
		void CreateCarrierIfReceiver(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal);
		ICarrier CreateInternalCarrier(ISemanticTypeStruct protocol, dynamic signal);
		void Remove(IReceptorInstance receptorInstance);
		List<string> GetProtocolsEndingWith(string match);
	}

	public interface ICarrier
	{
		ISemanticTypeStruct Protocol { get; set; }
		dynamic Signal { get; set; }
	}

	public interface IReceptor
	{
		string Name { get; }
		string AssemblyName { get; }
		IReceptorInstance Instance { get; }
		bool Enabled { get; set; }
	}

	/// <summary>
	/// A membrane has permeability to protocols.
	/// </summary>
	public enum PermeabilityDirection
	{
		In,
		Out,
	}

	public class PermeabilityConfiguration
	{
		public PermeabilityDirection Direction { get; set; }
		public bool Permeable { get; set; }
	}

	public interface IMembrane
	{
		event EventHandler<MembraneEventArgs> NewMembrane;
		event EventHandler<ReceptorEventArgs> NewReceptor;
		event EventHandler<NewCarrierEventArgs> NewCarrier;
		event EventHandler<ReceptorEventArgs> ReceptorRemoved;

		string Name { get; set; }
		ISemanticTypeSystem SemanticTypeSystem { get; }
		List<IMembrane> Membranes { get; }
		ReadOnlyCollection<IReceptor> Receptors { get; }
		Dictionary<string, PermeabilityConfiguration> ProtocolPermeability { get; }

		ICarrier CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal);
		void CreateCarrierIfReceiver(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal);
		ICarrier CreateInternalCarrier(ISemanticTypeStruct protocol, dynamic signal);
		void Remove(IReceptorInstance receptorInstance);
		void Reset();
		void Dissolve();
		void RegisterReceptor(string fn);
		void RegisterReceptor(string name, IReceptorInstance instance);
		void LoadReceptors(Action<IReceptor> afterRegister = null);
		void ProcessQueuedCarriers();
		
		IMembrane ParentMembrane { get; }
		List<string> GetEmittedProtocols();
		List<string> GetListeningProtocols();

		void UpdateMasterConnectionList(Dictionary<IReceptor, List<IReceptor>> masterList);
	}
}
