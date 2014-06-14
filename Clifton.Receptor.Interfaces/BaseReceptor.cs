using System;
using System.Collections.Generic;
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
	public abstract class BaseReceptor : IReceptorInstance
	{
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;

		public abstract string Name { get; }
		public virtual bool IsEdgeReceptor { get { return false; } }
		public virtual bool IsHidden { get { return false; } }

		protected List<string> receiveProtocols;
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
			receiveProtocols = new List<string>();
			emitProtocols = new List<string>();
		}

		public virtual void Initialize()
		{
		}

		public virtual void Terminate()
		{
		}

		public virtual string[] GetReceiveProtocols()
		{
			return receiveProtocols.ToArray();
		}

		public virtual string[] GetEmittedProtocols()
		{
			return emitProtocols.ToArray();
		}

		public abstract void ProcessCarrier(ICarrier carrier);

		protected virtual void AddReceiveProtocol(string p)
		{
			receiveProtocols.Add(p);
			ReceiveProtocolsChanged.Fire(this, EventArgs.Empty);
		}

		protected virtual void RemoveReceiveProtocol(string p)
		{
			receiveProtocols.Remove(p);
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
	}
}
