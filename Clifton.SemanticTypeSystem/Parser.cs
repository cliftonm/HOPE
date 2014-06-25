using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using Clifton.Assertions;
using Clifton.ExtensionMethods;

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	public static class Parser
	{
		public static List<SemanticTypeStruct> ParseStructs(string xml)
		{
			XDocument xdoc = XDocument.Parse(xml);
			var baseElement = xdoc.Element("SemanticTypes").Elements("Collection").Elements("SemanticTypeStruct");

			var structs = (from st in baseElement
						   select new SemanticTypeStruct()
						   {
							   DeclTypeName = st.Attribute("DeclType").Value,
							   NativeTypes = st.Elements("Attributes").IfNotNullReturn(t => 
								   from nativeType in t.Elements("NativeType")
										select new NativeType()
										{
											Name = nativeType.Attribute("Name").Value,
											ImplementingType = nativeType.Attribute("ImplementingType").Value,
										}).ToList<INativeType>(),
							   SemanticElements = st.Elements("Attributes").IfNotNullReturn(t => 
								   from semanticType in t.Elements("SemanticElement")
										select new SemanticElement()
										{
											Name = semanticType.Attribute("Name").Value,
										}).ToList<ISemanticElement>(),
						   }).ToList();

			return structs;
		}

		public static List<SemanticTypeDecl> ParseDeclarations(string xml)
		{
			XDocument xdoc = XDocument.Parse(xml);
			var baseElement = xdoc.Element("SemanticTypes").Elements("Collection").Elements("SemanticTypeDecl");

			var structs = (from st in baseElement
						   select new SemanticTypeDecl()
						   {
							   OfTypeName = st.Attribute("OfType").Value,
							   AttributeValues = st.Elements("AttributeValues").IfNotNullReturn(t => from nativeType in t.Elements("Attribute")
																									 select new AttributeValue()
																									 {
																										 Name = nativeType.Attribute("Name").Value,
																										 Value = (nativeType.Attribute("Value") != null) ? nativeType.Attribute("Value").Value : null,
																									 }).ToList<IAttributeValue>(),
						   }).ToList();

			return structs;
		}

		public static Dictionary<string, ISemanticType> BuildSemanticTypes(List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs)
		{
			Dictionary<string, ISemanticType> semanticTypes = new Dictionary<string, ISemanticType>();

			decls.ForEach(decl =>
			{
				// Given a SemanticTypeDecl:
				// The supporting SemanticTypeStruct is identified by the Value value of the attribute named "Name".
				// Must exist.  An exception is thrown otherwise.
				Assert.ErrorMessage = "Decl is missing a Name attribute";
				var structName = decl.AttributeValues.Single(attrValue => attrValue.Name == "Name").Value;

				// This gives us the SemanticTypeStruct:
				Assert.ErrorMessage = "Struct " + structName + " does not have a declaration.";
				var ststruct = structs.Single(s => s.DeclTypeName == structName);

				// The OfType struct in the decl is the same as the DeclType of a SemanticTypeStruct
				Assert.ErrorMessage = "Struct " + decl.OfTypeName + " does not have a declaration.";
				var ofType = structs.Single(s => s.DeclTypeName == decl.OfTypeName);
				Assert.ErrorMessage = null;

				// Set the cross-reference instances:
				ststruct.DeclType = decl;
				decl.OfType = ststruct;

				// The semantic type cannot exist.
				Assert.That(!semanticTypes.ContainsKey(structName), "Duplicate SemanticTypeStruct: " + structName);

				// The decl's of-type struct's native types must match the decl's attribute value names.
				// This is a validation process.
				Match(ofType, decl.AttributeValues);

				ISemanticType st = new SemanticType() { Decl = decl, Struct = ststruct };
				semanticTypes[structName] = st;
			});

			// Populate Element instances.
			structs.ForEach(s =>
			{
				s.SemanticElements.ForEach(elem => elem.Element = semanticTypes[elem.Name]);
			});

			return semanticTypes;
		}

		/// <summary>
		/// Each native type in the struct 
		/// </summary>
		public static void Match(SemanticTypeStruct ststruct, List<IAttributeValue> attrValues)
		{
			List<INativeType> nativeTypes = ststruct.NativeTypes;
			nativeTypes.ForEach(nt => Assert.That(attrValues.Any(attr => attr.Name == nt.Name), "Attribute mismatch between decl and struct for struct DeclType" + ststruct.DeclTypeName));
		}
	}
}
