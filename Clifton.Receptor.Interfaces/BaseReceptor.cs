using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor.Interfaces
{
	/// <summary>
	/// Useful to derive from this receptor when not needing to implement every single property / event handler.
	/// </summary>
	public abstract class BaseReceptor : IReceptorInstance, ISupportInitialize
	{
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;

		public abstract string Name { get; }
		public virtual bool IsEdgeReceptor { get { return false; } }
		public virtual bool IsHidden { get { return false; } }

		protected List<ReceiveQualifier> receiveProtocols;
		protected List<string> emitProtocols;

		public virtual IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;

		public BaseReceptor(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			receiveProtocols = new List<ReceiveQualifier>();
			emitProtocols = new List<string>();
		}

		public virtual void Initialize()
		{
		}

		public virtual void Terminate()
		{
		}

		public virtual List<ReceiveQualifier> GetReceiveProtocols()
		{
			return receiveProtocols;
		}

		public virtual List<string> GetEmittedProtocols()
		{
			return emitProtocols;
		}

		/// <summary>
		/// If not overridden, will invoke the action associated with the receive protocol that is qualified by the qualifier function.
		/// </summary>
		/// <param name="carrier"></param>
		public virtual void ProcessCarrier(ICarrier carrier)
		{
			ReceiveQualifier rq = receiveProtocols.Find(rp => rp.Protocol == carrier.Protocol.DeclTypeName && rp.Qualifier(carrier.Signal));
			rq.Action(carrier.Signal);
		}

		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}

		protected virtual void AddReceiveProtocol(string p)
		{
			receiveProtocols.Add(new ReceiveQualifier(p));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected virtual void AddReceiveProtocol(string p, Action<dynamic> a)
		{
			receiveProtocols.Add(new ReceiveQualifier(p, a));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected virtual void AddReceiveProtocol(string p, Func<dynamic, bool> q)
		{
			receiveProtocols.Add(new ReceiveQualifier(p, q));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected virtual void AddReceiveProtocol(string p, Func<dynamic, bool> q, Action<dynamic> a)
		{
			receiveProtocols.Add(new ReceiveQualifier(p, q, a));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected virtual void RemoveReceiveProtocol(string p)
		{
			receiveProtocols.Remove(receiveProtocols.Single(rp=>rp.Protocol==p));
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected virtual void AddEmitProtocol(string p)
		{
			emitProtocols.Add(p);
			EmitProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected virtual void RemoveEmitProtocol(string p)
		{
			emitProtocols.Remove(p);
			EmitProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected void CreateCarrier(string protocol, Action<dynamic> initializeSignal)
		{
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			dynamic outsignal = rsys.SemanticTypeSystem.Create(protocol);
			initializeSignal(outsignal);
			rsys.CreateCarrier(this, outprotocol, outsignal);
		}

		protected void CreateCarrierIfReceiver(string protocol, Action<dynamic> initializeSignal)
		{
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			dynamic outsignal = rsys.SemanticTypeSystem.Create(protocol);
			initializeSignal(outsignal);
			rsys.CreateCarrierIfReceiver(this, outprotocol, outsignal);
		}
	}
}
