using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Receptor;
using Clifton.SemanticTypeSystem;

using SemanticDatabase;

namespace SemanticDatabaseTests
{
	[TestClass]
	public class SemanticDatabaseTest
	{
		[TestMethod]
		public void SimpleInsert()
		{
			STS ssys = new STS();
			ReceptorsContainer rsys = new ReceptorsContainer(ssys);
			SemanticDatabaseReceptor sdr = new SemanticDatabaseReceptor(rsys);

			List<SemanticTypeDecl> decls = new List<SemanticTypeDecl>();
			List<SemanticTypeStruct> structs = new List<SemanticTypeStruct>();

			InitializeNoun(ssys, decls, structs);
			SemanticTypeStruct sts = CreateSemanticType("LatLon", false, decls, structs);
			CreateNativeType(sts, "latitude", "double", false);
			CreateNativeType(sts, "longitude", "double", false);
			
			ssys.Parse(decls, structs);
			string code = ssys.GenerateCode();
			System.Reflection.Assembly assy = Compiler.Compile(code);
			ssys.CompiledAssembly = assy;

			sdr.Protocols = "LatLon";
			sdr.ProtocolsUpdated();
		}

		protected void InitializeNoun(STS ssys, List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs)
		{
			// Reflective noun necessary for self-referential definition.
			SemanticTypeDecl decl = new SemanticTypeDecl() { OfTypeName = "Noun" };
			decl.AttributeValues.Add(new AttributeValue() { Name = "Name", Value = "Noun" });
			decls.Add(decl);

			SemanticTypeStruct sts = new SemanticTypeStruct() { DeclTypeName = "Noun" };
			sts.NativeTypes.Add(new Clifton.SemanticTypeSystem.NativeType() { Name = "Name", ImplementingType = "string" });
			structs.Add(sts);
		}

		protected SemanticTypeStruct CreateSemanticType(string name, bool unique, List<SemanticTypeDecl> decls, List<SemanticTypeStruct> structs)
		{
			SemanticTypeDecl decl = new SemanticTypeDecl() { OfTypeName = "Noun" };
			decl.AttributeValues.Add(new AttributeValue() { Name = "Name", Value = name });
			SemanticTypeStruct sts = new SemanticTypeStruct() { DeclTypeName = name, UniqueField = unique };

			decls.Add(decl);
			structs.Add(sts);

			return sts;
		}

		protected void CreateNativeType(SemanticTypeStruct sts, string name, string type, bool unique)
		{
			sts.NativeTypes.Add(new Clifton.SemanticTypeSystem.NativeType() { Name = name, ImplementingType = type, UniqueField = unique });
		}

		protected void CreateSemanticElement(SemanticTypeStruct sts, string name, string subtypeName, bool unique)
		{
			sts.SemanticElements.Add(new Clifton.SemanticTypeSystem.SemanticElement() { Name = name, UniqueField = unique });
		}
	}
}
