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

			// Initialize declaration and structure lists.
			decls = new List<SemanticTypeDecl>();
			structs = new List<SemanticTypeStruct>();

			// We must have a noun definition for now.
			Helpers.InitializeNoun(ssys, decls, structs);

			// We need this ST for query tests.
			SemanticTypeStruct sts = Helpers.CreateSemanticType("Query", false, decls, structs);
			Helpers.CreateNativeType(sts, "QueryText", "string", false);

			// Initialize the Semantic Database Receptor
			sdr = new SemanticDatabaseReceptor(rsys);

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

		protected void InitRestaurantLatLonUniqueParentSTElement()
		{
			SemanticTypeStruct stsRest = Helpers.CreateSemanticType("Restaurant", false, decls, structs);
			SemanticTypeStruct stsLatLon = Helpers.CreateSemanticType("LatLon", false, decls, structs);			// child ST LatLon is declared to NOT be unique.
			Helpers.CreateNativeType(stsLatLon, "latitude", "double", false);
			Helpers.CreateNativeType(stsLatLon, "longitude", "double", false);
			Helpers.CreateSemanticElement(stsRest, "LatLon", true);											// The element LatLon in Restaurant is unique.
		}

		protected void InitRestaurantUniqueLatLonAndParentSTElement()
		{
			SemanticTypeStruct stsRest = Helpers.CreateSemanticType("Restaurant", true, decls, structs);
			
			// child ST LatLon is NOT declared to be unique, but is actually treated as unique because Restaurant, the parent ST, is declared unique.
			SemanticTypeStruct stsLatLon = Helpers.CreateSemanticType("LatLon", false, decls, structs);			

			Helpers.CreateNativeType(stsLatLon, "latitude", "double", false);
			Helpers.CreateNativeType(stsLatLon, "longitude", "double", false);
			Helpers.CreateSemanticElement(stsRest, "LatLon", true);											// The element LatLon in Restaurant is unique.
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
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");

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
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");

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
			Assert.AreEqual(2, count, "Expected 2 LatLon record.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(2, count, "Expected 2 Restaurant records.");

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
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(2, count, "Expected 2 Restaurant records.");

			sdr.Terminate();
		}

		/// <summary>
		/// Verifies that a two-tier ST structure with a child whose ST is not unique but the parent ST's element referencing the child is unique.
		/// Because the child is not unique, this should result in two parent entries for same child entry, since the child FK_ID will be different.
		/// </summary>
		[TestMethod]
		public void TwoLevelUniqueSTElementInsert()
		{
			InitializeSDRTests(() => InitRestaurantLatLonUniqueParentSTElement());

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
			Assert.AreEqual(2, count, "Expected 2 Restaurant records.");

			sdr.Terminate();
		}

		/// <summary>
		/// Verifies that a two-tier ST structure with a child whose ST is unique and the parent ST's element referencing the child is unique.
		/// Because the child is unique and the parent's element referencing the child is unique, this should result in one parent and one child entry for the same data.
		/// </summary>
		[TestMethod]
		public void TwoLevelUniqueElementAndChildSTInsert()
		{
			InitializeSDRTests(() => InitRestaurantUniqueLatLonAndParentSTElement());

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
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 Restaurant record.");

			sdr.Terminate();
		}

		/// <summary>
		/// Here we designate the parent to be unique.  This will test the parent's composite fields for uniqueness.  However,
		/// the only way the parent's composite fields can ever be unique is if we enforce a uniqueness test in all the child ST's as well.
		/// </summary>
		[TestMethod]
		public void TwoLevelUniqueParentSTInsert()
		{
			InitializeSDRTests(() => InitRestaurantUniqueLatLonAndParentSTElement());

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
			Assert.AreEqual(1, count, "Expected 1 LatLon record.");
			cmd.CommandText = "SELECT count(*) from Restaurant";
			count = Convert.ToInt32(cmd.ExecuteScalar());
			Assert.AreEqual(1, count, "Expected 1 Restaurant record.");

			sdr.Terminate();
		}

		[TestMethod]
		public void SimpleQuery()
		{
			InitializeSDRTests(() => InitLatLonNonUnique());

			// Initialize the Semantic Data Receptor with the signal it should be listening to.
			DropTable("LatLon");
			sdr.Protocols = "LatLon";
			sdr.ProtocolsUpdated();

			// Create the signal.
			ICarrier latLonCarrier = Helpers.CreateCarrier(rsys, "LatLon", signal =>
			{
				signal.latitude = 1.0;
				signal.longitude = 2.0;
			});

			sdr.ProcessCarrier(latLonCarrier);

			// Create the query
			ICarrier queryCarrier = Helpers.CreateCarrier(rsys, "Query", signal =>
			{
				signal.QueryText = "LatLon";
			});

			sdr.ProcessCarrier(queryCarrier);
			List<QueuedCarrierAction> queuedCarriers = rsys.QueuedCarriers;
			Assert.AreEqual(1, queuedCarriers.Count, "Expected one signal to be returned.");
			dynamic retSignal = queuedCarriers[0].Carrier.Signal;
			Assert.AreEqual(1.0, retSignal.latitude, "Wrong data.");
			Assert.AreEqual(2.0, retSignal.longitude, "Wrong data.");
		}
	}
}
