using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;

/// Serializeable / Deserializable classes
namespace TypeSystemExplorer.Models
{
	public class Applet
	{
		public MembranesDef membranesDef;
		public CarriersDef carriersDef;

		public MembranesDef MembranesDef
		{
			get { return membranesDef; }
			set { membranesDef = value; }
		}

		public CarriersDef CarriersDef
		{
			get { return carriersDef; }
			set { carriersDef = value; }
		}

		public Applet()
		{
			// TODO: Why does setting these properties cause MycroParser to assert?
			// membranesDef = new MembranesDef();
			// carriersDef = new CarriersDef();
		}
	}

	public class MembranesDef
	{
		protected List<MembraneDef> membranes;

		public List<MembraneDef> Membranes
		{
			get { return membranes; }
			set { membranes = value; }
		}

		public MembranesDef()
		{
			membranes = new List<MembraneDef>();
		}
	}

	public class PermeabilityDef
	{
		public string Protocol { get; set; }
		public PermeabilityDirection Direction { get; set; }
		public bool Permeable { get; set; }
	}

	public class MembraneDef
	{
		protected List<ReceptorDef> receptors;
		protected List<MembraneDef> membranes;
		protected List<PermeabilityDef> permeabilities;

		public string Name { get; set; }

		public List<ReceptorDef> Receptors
		{
			get { return receptors; }
			set { receptors = value; }
		}

		public List<MembraneDef> Membranes
		{
			get { return membranes; }
			set { membranes = value; }
		}

		public List<PermeabilityDef> Permeabilities
		{
			get { return permeabilities; }
			set { permeabilities = value; }
		}

		public MembraneDef()
		{
			Receptors = new List<ReceptorDef>();
			Membranes = new List<MembraneDef>();
			Permeabilities = new List<PermeabilityDef>();
		}
	}

	public class ReceptorDef
	{
		public string Name { get; set; }
		public string AssemblyName { get; set; }
		public Point Location { get; set; }
		public bool Enabled { get; set; }

		public ReceptorDef()
		{
			// Default.
			Enabled = true;
		}
	}

	public class CarriersDef
	{
		protected List<CarrierDef> carriers;

		public List<CarrierDef> Carriers
		{
			get { return carriers; }
			set { carriers = value; }
		}

		public CarriersDef()
		{
			carriers = new List<CarrierDef>();
		}
	}

	public class CarrierDef
	{
		public string Protocol { get; set; }
		public string Membrane { get; set; }

		protected List<Attr> attributes;

		public List<Attr> Attributes
		{
			get { return attributes; }
			set { attributes = value; }
		}

		public CarrierDef()
		{
			attributes = new List<Attr>();
		}
	}

	public class Attr
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}
}
