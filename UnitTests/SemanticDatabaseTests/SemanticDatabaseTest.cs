using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Clifton.Receptor;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;

using SemanticDatabase;

namespace SemanticDatabaseTests
{
	[TestClass]
	public class SemanticDatabaseTest
	{
		protected STS ssys;
		protected ReceptorsContainer rsys;
		protected SemanticDatabaseReceptor sdr;
		protected List<SemanticTypeDecl> decls;
		protected List<SemanticTypeStruct> structs;

		protected void InitializeSDRTests(Action initStructs)
		{
			// Initialize the Semantic Type System.
			ssys = new STS();

			// Initialize the Receptor System
			rsys = new ReceptorsContainer(ssys);

			// Initialize the Semantic Database Receptor
			sdr = new SemanticDatabaseReceptor(rsys);

			// Initialize declaration and structure lists.
			decls = new List<SemanticTypeDecl>();
			structs = new List<SemanticTypeStruct>();

			// We must have a noun definition for now.
			Helpers.InitializeNoun(ssys, decls, structs);

			// Create our semantic structure.
			initStructs();

			// Instantiate the runtime code-behind.
			ssys.Parse(decls, structs);
			string code = ssys.GenerateCode();
			System.Reflection.Assembly assy = Compiler.Compile(code);
			ssys.CompiledAssembly = assy;
		}

		protected void InitLatLonNonUnique()
		{
			SemanticTypeStruct sts = Helpers.CreateSemanticType("LatLon", false, decls, structs);
			Helpers.CreateNativeType(sts, "latitude", "double", false);
			Helpers.CreateNativeType(sts, "longitude", "double", false);
		}

		protected void InitRestaurantLatLonNonUnique()
		{
			SemanticTypeStruct stsRest = Helpers.CreateSemanticType("Restaurant", false, decls, structs);
			SemanticTypeStruct stsLatLon = Helpers.CreateSemanticType("LatLon", false, decls, structs);
			Helpers.CreateNativeType(stsLatLon, "latitude", "double", false);
			Helpers.CreateNativeType(stsLatLon, "longitude", "double", false);
			Helpers.CreateSemanticElement(stsRest, "LatLon", false);
		}

		protected void InitRestaurantLatLonUniqueChildST()
		{
			SemanticTypeStruct stsRest = Helpers.CreateSemanticType("Restaurant", false, decls, structs);
			SemanticTypeStruct stsLatLon = Helpers.CreateSemanticType("LatLon", true, decls, structs);			// child ST LatLon is declared to be unique.
			Helpers.CreateNativeType(stsLatLon, "latitude", "double", false);
			Helpers.CreateNativeType(stsLatLon, "longitude", "double", false);
			Helpers.CreateSemanticElement(stsRest, "LatLon", false);											// The element LatLon in Restaurant is NOT unique.
		}

		// TODO: We should also test a unique single field in an multi-field ST.
		protected void InitLatLonUniqueFields()
		{
			SemanticTypeStruct sts = Helpers.CreateSemanticType("LatLon", false, decls, structs);
			Helpers.CreateNativeType(sts, "latitude", "double", true);
			Helpers.CreateNativeType(sts, "longitude", "double", true);
		}

		protected void InitLatLonUniqueST()
		{
			SemanticTypeStruct sts = Helpers.CreateSemanticType("LatLon", true, decls, structs);
			Helpers.CreateNativeType(sts, "latitude", "double", false);
			Helpers.CreateNativeType(sts, "longitude", "double", false);
		}

		protected void DropTable(string tableName)
		{
			SQLiteConnection conn = sdr.Connection;
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "drop table "+tableName;

			try
			{
				cmd.ExecuteNonQuery();
			}
			catch
			{
				// Ignore missing table exceptions
			}
		}

		/// <summary>
		/// Verifies that a non-unique ST with 2 NT's generates multiple records for the same data.
		/// </summary>
		[TestMethod]
		public void SimpleNonUniqueInsert()
		{
			InitializeSDRTests(() => InitLatLonNonUnique());

			// Initialize the Semantic Data Receptor with the signal it should be listening to.
			DropTable("LatLon");
			sdr.Protocols = "LatLon";
			sdr.ProtocolsUpdated();

			// Create the signal.
			ICarrier carrier = Helpers.CreateCarrier(rsys, "LatLon", signal =>
				{
					signal.latitude = 1.0;
					signal.longitude = 2.0;
				});

			// Let's see what the SDR does.
			sdr.ProcessCarrier(carrier);
			SQLiteConnection conn = sdr.Connection;

			int count;
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");

			// Insert another, identical record.  We should now have two records.
			sdr.ProcessCarrier(carrier);
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(2, count, "Expected 2 LatLon records.");

			sdr.Terminate();
		}

		/// <summary>
		/// Verifies that an ST with two unique NT's generates only a single record for the same data.
		/// </summary>
		[TestMethod]
		public void SimpleUniqueFieldsInsert()
		{
			InitializeSDRTests(() => InitLatLonUniqueFields());

			// Initialize the Semantic Data Receptor with the signal it should be listening to.
			DropTable("LatLon");
			sdr.Protocols = "LatLon";
			sdr.ProtocolsUpdated();

			// Create the signal.
			ICarrier carrier = Helpers.CreateCarrier(rsys, "LatLon", signal =>
			{
				signal.latitude = 1.0;
				signal.longitude = 2.0;
			});

			// Let's see what the SDR does.
			sdr.ProcessCarrier(carrier);
			SQLiteConnection conn = sdr.Connection;

			int count;
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");

			// Insert another, identical record.  We should still have one record.
			sdr.ProcessCarrier(carrier);
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon records.");

			sdr.Terminate();
		}

		/// <summary>
		/// Verifies that a unique ST with two NT's generates only a single record for the same data.
		/// </summary>
		[TestMethod]
		public void SimpleUniqueSTInsert()
		{
			InitializeSDRTests(() => InitLatLonUniqueST());

			// Initialize the Semantic Data Receptor with the signal it should be listening to.
			DropTable("LatLon");
			sdr.Protocols = "LatLon";
			sdr.ProtocolsUpdated();

			// Create the signal.
			ICarrier carrier = Helpers.CreateCarrier(rsys, "LatLon", signal =>
			{
				signal.latitude = 1.0;
				signal.longitude = 2.0;
			});

			// Let's see what the SDR does.
			sdr.ProcessCarrier(carrier);
			SQLiteConnection conn = sdr.Connection;

			int count;
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");

			// Insert another, identical record.  We should still have one record.
			sdr.ProcessCarrier(carrier);
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon records.");

			sdr.Terminate();
		}

		/// <summary>
		/// Verifies that a two-tier ST structure with no unique fields creates duplicate entries of the parent and child ST's.
		/// </summary>
		[TestMethod]
		public void TwoLevelNonUniqueSTInsert()
		{
			InitializeSDRTests(() => InitRestaurantLatLonNonUnique());

			// Initialize the Semantic Data Receptor with the signal it should be listening to.
			DropTable("Restaurant");
			DropTable("LatLon");
			sdr.Protocols = "LatLon; Restaurant";
			sdr.ProtocolsUpdated();

			// Create the signal.
			ICarrier carrier = Helpers.CreateCarrier(rsys, "Restaurant", signal =>
			{
				signal.LatLon.latitude = 1.0;
				signal.LatLon.longitude = 2.0;
			});

			// Let's see what the SDR does.
			sdr.ProcessCarrier(carrier);
			SQLiteConnection conn = sdr.Connection;

			int count;
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 Restaurant record.");

			// Insert another, identical record.  We should still have one record.
			sdr.ProcessCarrier(carrier);
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(2, count, "Expected 2 LatLon records.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(2, count, "Expected 2 Restaurant record.");

			sdr.Terminate();
		}

		/// <summary>
		/// Verifies that a two-tier ST structure with a child whose ST is defined as unique creates a single entry for the child and two entries for the parent.
		/// </summary>
		[TestMethod]
		public void TwoLevelUniqueChildSTInsert()
		{
			InitializeSDRTests(() => InitRestaurantLatLonUniqueChildST());

			// Initialize the Semantic Data Receptor with the signal it should be listening to.
			DropTable("Restaurant");
			DropTable("LatLon");
			sdr.Protocols = "LatLon; Restaurant";
			sdr.ProtocolsUpdated();

			// Create the signal.
			ICarrier carrier = Helpers.CreateCarrier(rsys, "Restaurant", signal =>
			{
				signal.LatLon.latitude = 1.0;
				signal.LatLon.longitude = 2.0;
			});

			// Let's see what the SDR does.
			sdr.ProcessCarrier(carrier);
			SQLiteConnection conn = sdr.Connection;

			int count;
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 Restaurant record.");

			// Insert another, identical record.  We should still have one record.
			sdr.ProcessCarrier(carrier);
			cmd.CommandText = "SELECT count(*) from LatLon";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 LatLon records.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(2, count, "Expected 2 Restaurant record.");

			sdr.Terminate();
		}
	}
}
