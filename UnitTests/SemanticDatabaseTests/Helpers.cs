using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;

namespace SemanticDatabaseTests
{
	public static class Helpers
	{
		public static void InitializeNoun(STS ssys, List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs)
		{
			// Reflective noun necessary for self-referential definition.
			SemanticTypeDecl decl = new SemanticTypeDecl() { OfTypeName = "Noun" };
			decl.AttributeValues.Add(new AttributeValue() { Name = "Name", Value = "Noun" });
			decls.Add(decl);

			SemanticTypeStruct sts = new SemanticTypeStruct() { DeclTypeName = "Noun" };
			sts.NativeTypes.Add(new Clifton.SemanticTypeSystem.NativeType() { Name = "Name", ImplementingType = "string" });
			structs.Add(sts);
		}

		/// <summary>
		/// Creates a root semantic type.
		/// </summary>
		public static SemanticTypeStruct CreateSemanticType(string name, bool unique, List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs)
		{
			SemanticTypeDecl decl = new SemanticTypeDecl() { OfTypeName = "Noun" };
			decl.AttributeValues.Add(new AttributeValue() { Name = "Name", Value = name });
			SemanticTypeStruct sts = new SemanticTypeStruct() { DeclTypeName = name, Unique = unique };

			decls.Add(decl);
			structs.Add(sts);

			return sts;
		}

		/// <summary>
		/// Creates a native type for the specified ST (sts).
		/// </summary>
		public static void CreateNativeType(SemanticTypeStruct sts, string name, string type, bool unique)
		{
			sts.NativeTypes.Add(new Clifton.SemanticTypeSystem.NativeType() { Name = name, ImplementingType = type, UniqueField = unique });
		}

		/// <summary>
		/// Associates an ST as a sub-element of the specified parent ST (sts).
		/// </summary>
		public static void CreateSemanticElement(SemanticTypeStruct sts, string subtypeName, bool unique)
		{
			sts.SemanticElements.Add(new Clifton.SemanticTypeSystem.SemanticElement() { Name = subtypeName, UniqueField = unique });
		}

		public static ICarrier CreateCarrier(ReceptorsContainer rsys, string protocol, Action<dynamic> initializeSignal)
		{
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			dynamic outsignal = rsys.SemanticTypeSystem.Create(protocol);
			initializeSignal(outsignal);
			ICarrier carrier = new Carrier(outprotocol, "", outsignal, null);

			return carrier;
		}
	}
}
