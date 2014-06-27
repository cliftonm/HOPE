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
	/// Receptors can define user configurable properties with this attribute.
	/// Double-clicking on the receptor will bring up a simple UI for the user to configure
	/// these properties.  The property values are persisted when serialized and restored
	/// when deserialized.
	/// </summary>
	public class UserConfigurablePropertyAttribute : Attribute
	{
		protected string prompt;

		public UserConfigurablePropertyAttribute(string prompt)
		{
			this.prompt = prompt;
		}
	}	

	/// <summary>
	/// All receptors must implement this interface in order to be dynamically loaded at runtime.
	/// </summary>
	public interface IReceptorInstance
	{
		event EventHandler<EventArgs> ReceiveProtocolsChanged;
		event EventHandler<EventArgs> EmitProtocolsChanged;

		string Name { get; }
		bool IsEdgeReceptor { get; }
		bool IsHidden { get; }

		// The receptor system must be reset when a receptor moves to a different membrane,
		// that is, to another receptor system.
		IReceptorSystem ReceptorSystem { get; set; }

		List<ReceiveQualifier> GetReceiveProtocols();
		List<string> GetEmittedProtocols();
		void ProcessCarrier(ICarrier carrier);

		/// <summary>
		/// Called when the receptor initialization has completed.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Called when the system shuts down, allowing receptor instances to dispose of unmanaged resources and 
		/// perform other cleanup.
		/// Also called when a new applet is loaded.
		/// </summary>
		void Terminate();

		/// <summary>
		/// Called when the entire system has been initialized, after loading an applet.
		/// Called on an individual receptor when it is dragged and dropped onto an existing surface.
		/// </summary>
		void EndSystemInit();
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

	/// <summary>
	/// The default construction assumes false for Permeable.
	/// </summary>
	public class PermeabilityConfiguration
	{
		public bool Permeable { get; set; }
	}

	public struct PermeabilityKey
	{
		public string Protocol { get; set; }
		public PermeabilityDirection Direction { get; set; }
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
		Dictionary<PermeabilityKey, PermeabilityConfiguration> ProtocolPermeability { get; }

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
