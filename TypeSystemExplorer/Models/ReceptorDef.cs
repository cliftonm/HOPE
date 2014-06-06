using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	public class MembraneDef
	{
		protected List<ReceptorDef> receptors;

		public List<ReceptorDef> Receptors
		{
			get { return receptors; }
			set { receptors = value; }
		}

		public MembraneDef()
		{
			Receptors = new List<ReceptorDef>();
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
