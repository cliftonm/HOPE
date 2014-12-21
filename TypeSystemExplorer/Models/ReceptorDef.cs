/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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
		public Point SurfaceOffset { get; set; }

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
		public bool RootOnly { get; set; }
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

	public class UserConfig
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	public class ReceiveProtocol
	{
		public string Protocol { get; set; }
		public bool Enabled { get; set; }
	}

	public class EmitProtocol
	{
		public string Protocol { get; set; }
		public bool Enabled { get; set; }
	}

	public class ReceptorDef
	{
		public string Name { get; set; }
		public string AssemblyName { get; set; }
		public Point Location { get; set; }
		public bool Enabled { get; set; }
		public List<ReceiveProtocol> ReceiveProtocols {get; set;}
		public List<EmitProtocol> EmitProtocols { get; set; }
		public List<UserConfig> UserConfigs { get; set; }

		public ReceptorDef()
		{
			// Default.
			UserConfigs = new List<UserConfig>();
			ReceiveProtocols = new List<ReceiveProtocol>();
			EmitProtocols = new List<EmitProtocol>();
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
