using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace CarrierExporterReceptor
{
	/// <summary>
	/// A universal carrier receptor -- it receives all carriers and exports their contents to XML, so that we can extract specific carriers for testing purposes.
	/// </summary>
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Carrier Exporter"; } }
		public bool IsEdgeReceptor { get { return true; } }
		public bool IsHidden { get { return false; } }

		public IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;
		protected XmlDocument xdoc;
		protected XmlNode carriersNode;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			xdoc = new XmlDocument();
			carriersNode = xdoc.AppendChild(xdoc.CreateElement("Carriers"));
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "*" };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { };
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
			XmlWriterSettings xws = new XmlWriterSettings();
			xws.Indent = true;
			xws.OmitXmlDeclaration = true;
			XmlWriter xw = XmlWriter.Create("carrier_output.xml", xws);
			xdoc.WriteTo(xw);
			xw.Close();
		}

		public void ProcessCarrier(ICarrier carrier)
		{
			XmlNode node = xdoc.CreateElement("Carrier");
			node.Attributes.Append(CreateAttribute(xdoc, "Protocol", carrier.Protocol.DeclTypeName));
			carriersNode.AppendChild(node);
			ProcessCarrier(carrier, node);
		}

		protected void CreateSubNodes(XmlDocument xdoc, XmlNode node, string name, List<dynamic> val)
		{
			XmlNode subnode = xdoc.CreateElement(name);
			node.AppendChild(subnode);

			val.ForEach(item =>
				{
					Type t = item.GetType();
					XmlNode carrier = xdoc.CreateElement(t.Name);
					subnode.AppendChild(carrier);

					t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ForEach(p =>
					{
						object val2 = p.GetValue(item);

						if (val2 != null)
						{
							// If this is a collection...
							if (val2 is List<dynamic>)
							{
								// Must be a collection of semantic types!
								CreateSubNodes(xdoc, subnode, p.Name, (List<dynamic>)val2);
							}
							else if (val2 is ICarrier)
							{
								CreateSubNodes(xdoc, subnode, p.Name, (ICarrier)val2);
							}
							else if (val2 is IRuntimeSemanticType)
							{
								CreateSubNodes(xdoc, subnode, p.Name, (IRuntimeSemanticType)val2);
							}
							else
							{
								carrier.Attributes.Append(CreateAttribute(xdoc, p.Name, val2.ToString()));
							}
						}
					});
				});
		}

		protected void CreateSubNodes(XmlDocument xdoc, XmlNode node, string name, IRuntimeSemanticType val)
		{
			XmlNode subnode = xdoc.CreateElement(name);
			node.AppendChild(subnode);

			Type t = val.GetType();
			t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ForEach(p =>
				{
					object val2 = p.GetValue(val);

					if (val2 != null)
					{
						// If this is a collection...
						if (val2 is List<dynamic>)
						{
							// Must be a collection of semantic types!
							CreateSubNodes(xdoc, subnode, p.Name, (List<dynamic>)val2);
						}
						else if (val2 is ICarrier)
						{
							CreateSubNodes(xdoc, subnode, p.Name, (ICarrier)val2);
						}
						else if (val2 is IRuntimeSemanticType)
						{
							CreateSubNodes(xdoc, subnode, p.Name, (IRuntimeSemanticType)val2);
						}
						else
						{
							subnode.Attributes.Append(CreateAttribute(xdoc, p.Name, val2.ToString()));
						}
					}
				});
		}

		protected void CreateSubNodes(XmlDocument xdoc, XmlNode node, string name, ICarrier carrier)
		{
			XmlNode subnode = xdoc.CreateElement(name);
			node.AppendChild(subnode);
			node.Attributes.Append(CreateAttribute(xdoc, "Protocol", carrier.Protocol.DeclTypeName));
			carriersNode.AppendChild(node);
			ProcessCarrier(carrier, subnode);
		}

		protected XmlAttribute CreateAttribute(XmlDocument xdoc, string attrName, string value)
		{
			XmlAttribute xa = xdoc.CreateAttribute(attrName);
			xa.Value = value;

			return xa;
		}

		protected void ProcessCarrier(ICarrier carrier, XmlNode node)
		{
			Type t = carrier.Signal.GetType();
			t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ForEach(p =>
			{
				object val = p.GetValue(carrier.Signal);

				if (val != null)
				{
					// If this is a collection...
					if (val is List<dynamic>)
					{
						// Must be a collection of semantic types!
						CreateSubNodes(xdoc, node, p.Name, (List<dynamic>)val);
					}
					else if (val is ICarrier)
					{
						CreateSubNodes(xdoc, node, p.Name, (ICarrier)val);
					}
					else if (val is IRuntimeSemanticType)
					{
						CreateSubNodes(xdoc, node, p.Name, (IRuntimeSemanticType)val);
					}
					else
					{
						node.Attributes.Append(CreateAttribute(xdoc, p.Name, val.ToString()));
					}
				}
			});
		}
	}
}
