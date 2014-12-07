using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.MycroParser;
using Clifton.SemanticTypeSystem.Interfaces;

using Hope.Interfaces;

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
	/// We use this interface to get access to the application controller so that receptors can interact with the application's UI,
	/// for example, by adding their specific "UI" to the applet dock container.
	/// </summary>
	public interface IApplicationController
	{
		void AddAppletUI(object doc, Guid layoutId);
		void RemoveAppletUI(Guid layoutId);
		void UpdateAppletCaption(string title, Guid layoutId);
	}

	/// <summary>
	/// All receptors must implement this interface in order to be dynamically loaded at runtime.
	/// </summary>
	public interface IReceptorInstance
	{
		event EventHandler<EventArgs> ReceiveProtocolsChanged;
		event EventHandler<EventArgs> EmitProtocolsChanged;
		event EventHandler<EventArgs> SubnameChanged;

		string Name { get; }
		string Subname { get; }
		bool IsEdgeReceptor { get; }
		bool IsHidden { get; }
		bool Enabled { get; set; }
		string ConfigurationUI { get; }
		string ConfigurationError { get; }

		// The receptor system must be reset when a receptor moves to a different membrane,
		// that is, to another receptor system.
		IReceptorSystem ReceptorSystem { get; set; }

		List<ReceiveQualifier> GetReceiveProtocols();
		List<EmittedProtocol> GetEmittedProtocols();
		List<ReceiveQualifier> GetEnabledReceiveProtocols();
		List<EmittedProtocol> GetEnabledEmittedProtocols();

		void CacheEmitProtocol(string protocolName, bool enabled);
		void CacheReceiveProtocol(string protocolName, bool enabled);

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


		/// <summary>
		/// Called when the user configurable items in a receptor instance have been updated by user or other action.
		/// </summary>
		bool UserConfigurationUpdated();

		/// <summary>
		/// Called before the controls are populated, to give the receptor instance a chance to do other initialization 
		/// of the configuration UI.
		/// </summary>
		/// <param name="mp"></param>
		void PrepopulateConfig(Clifton.MycroParser.MycroParser mp);
	}

	public interface IReceptorSystem
	{
		ISemanticTypeSystem SemanticTypeSystem { get; }
		ICarrier CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal, ICarrier parentCarrier = null, bool emitSubElements = true);
		void CreateCarrierIfReceiver(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal, ICarrier parentCarrier = null, bool emitSubElements = true);
		void Remove(IReceptorInstance receptorInstance);
		List<string> GetProtocolsEndingWith(string match);
		void EndSystemInit();
		IMembrane Membrane { get; }
	}

	public interface ICarrier
	{
		ISemanticTypeStruct Protocol { get; set; }
		dynamic Signal { get; set; }
		string ProtocolPath { get; set; }
		ICarrier ParentCarrier { get; }
	}

	public interface IReceptor
	{
		string Name { get; }
		string AssemblyName { get; }
		IReceptorInstance Instance { get; }

		// Used in deserialization only, passed on to the instance when the receptor's implementing instance is created.
		bool Enabled { get; set; }
	}

	public interface IReceptorConnection
	{
		IReceptor Receptor { get; set; }
		bool RootOnly { get; set; }
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

		/// <summary>
		/// If true, only allows protocols whose root is of the "key" type (see PermeabilityKey below).
		/// </summary>
		public bool RootOnly { get; set; }
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

		IApplicationController ApplicationController { get; set; }

		string Name { get; set; }
		ISemanticTypeSystem SemanticTypeSystem { get; }
		List<IMembrane> Membranes { get; }
		ReadOnlyCollection<IReceptor> Receptors { get; }
		Dictionary<PermeabilityKey, PermeabilityConfiguration> ProtocolPermeability { get; }

		ICarrier CreateCarrier(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal);
		void CreateCarrierIfReceiver(IReceptorInstance from, ISemanticTypeStruct protocol, dynamic signal);
		void Remove(IReceptorInstance receptorInstance);
		void Reset();
		void EndSystemInit();
		void Dissolve();
		IReceptor RegisterReceptor(string fn);
		void RegisterReceptor(string name, IReceptorInstance instance);
		void LoadReceptors(Action<IReceptor> afterRegister = null);
		void ProcessQueuedCarriers();
		void UpdatePermeability();

		IMembrane ParentMembrane { get; set;  }
		List<string> GetEmittedProtocols();
		List<string> GetListeningProtocols();

		void UpdateMasterConnectionList(Dictionary<IReceptor, List<IReceptorConnection>> masterList);
	}
}
