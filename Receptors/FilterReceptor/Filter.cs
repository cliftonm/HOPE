using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NCalc;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

namespace FilterReceptor
{
    public class Filter : BaseReceptor
    {
		public override string Name { get { return "Filter"; } }
		public override bool IsEdgeReceptor { get { return false; } }
		public override string ConfigurationUI { get { return "FilterConfig.xml"; } }

		protected DataGridView dgvFilters;
		protected DataTable dt;
		protected Form form;
		
		protected Dictionary<string, List<string>> protocolFilterMap;

		[UserConfigurableProperty("Internal")]
		public string ProtocolFilters { get; set; }

		public Filter(IReceptorSystem rsys)
			: base(rsys)
		{
			protocolFilterMap = new Dictionary<string, List<string>>();
		}

		public override void Initialize()
		{
			base.Initialize();
			dt = InitializeDataTable();
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();
			InitializeFromConfig();
			InitializeReceiveAndEmitProtocols();
/*
			// test a protocol:
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("AlchemyConcept");
			dynamic outsignal = rsys.SemanticTypeSystem.Create("AlchemyConcept");
			outsignal.text = "Linux";
			ICarrier carrier = rsys.CreateInternalCarrier(outprotocol, outsignal);
			ProcessCarrier(carrier);
 */ 
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			base.ProcessCarrier(carrier);

			List<string> filters;

			if (protocolFilterMap.TryGetValue(carrier.Protocol.DeclTypeName, out filters))
			{
				FilterSignal(carrier.Protocol.DeclTypeName, carrier.Signal, filters);
			}
		}

		public override void PrepopulateConfig(MycroParser mp)
		{
			base.PrepopulateConfig(mp);
			dgvFilters = (DataGridView)mp.ObjectCollection["dgvFilters"];

			if (dt == null)
			{
				// Create the table if it doesn't exist, otherwise
				// we populate the grid with the existing configuration.
				dt = InitializeDataTable();
			}

			dgvFilters.DataSource = new DataView(dt);

			// Stupid DGV doesn't pay attention to column captions:
			foreach (DataGridViewColumn col in dgvFilters.Columns)
			{
				col.HeaderText = dt.Columns[col.HeaderText].Caption;
			}
		}

		/// <summary>
		/// Update the configuration property.
		/// </summary>
		public override bool UserConfigurationUpdated()
		{
			StringBuilder sb = new StringBuilder();
			string and = String.Empty;
			protocolFilterMap.Clear();

			foreach (DataRow row in dt.Rows)
			{
				string protocol = row[0].ToString();
				string filter = row[1].ToString();

				if (!String.IsNullOrEmpty(filter) && !String.IsNullOrEmpty(protocol))
				{
					// We use '~' and ';' internally, so strip those out.  Sorry user!
					filter = filter.Replace("~", "").Replace(";", "");
					protocol = protocol.Replace("~", "").Replace(";", "");
					AddToProtocolFilterMap(protocol, filter);

					// the format is [tabname],[protocolname];[tabname],[protocolname];...etc...
					sb.Append(and);
					sb.Append(protocol + "~" + filter);
					and = ";";
				}
			}

			ProtocolFilters = sb.ToString();
			InitializeReceiveAndEmitProtocols();

			return true;
		}

		/// <summary>
		/// Initialize the underlying table for the datagridview.
		/// </summary>
		/// <returns></returns>
		protected DataTable InitializeDataTable()
		{
			DataTable dt = new DataTable();
			DataColumn dcProtocol = new DataColumn("protocol");
			dcProtocol.Caption = "Protocol";
			DataColumn dcTabName = new DataColumn("filter");
			dcTabName.Caption = "Filter Expression";
			dt.Columns.AddRange(new DataColumn[] { dcProtocol, dcTabName });

			return dt;
		}

		/// <summary>
		/// Initializes the table used for configuration with the serialized protocol-filter list.
		/// </summary>
		protected void InitializeFromConfig()
		{
			if (!String.IsNullOrEmpty(ProtocolFilters))
			{
				string[] tpArray = ProtocolFilters.Split(';');

				foreach (string tp in tpArray)
				{
					string protocolName = tp.LeftOf('~');
					string filter = tp.RightOf('~');
					DataRow row = dt.NewRow();
					row[0] = protocolName;
					row[1] = filter;
					dt.Rows.Add(row);
					AddToProtocolFilterMap(protocolName, filter);
				}
			}
		}

		protected void AddToProtocolFilterMap(string protocolName, string filter)
		{
			List<string> filters;

			if (!protocolFilterMap.TryGetValue(protocolName, out filters))
			{
				filters = new List<string>();
				protocolFilterMap[protocolName] = filters;
			}

			protocolFilterMap[protocolName].Add(filter);
		}

		/// <summary>
		/// Initialize the filter to receive and emit the protocols in the configuration.
		/// </summary>
		protected void InitializeReceiveAndEmitProtocols()
		{
			RemoveEmitProtocols();
			RemoveReceiveProtocols();

			foreach (DataRow row in dt.Rows)
			{
				string protocol = row[0].ToString();
				AddReceiveProtocol(protocol);
				// TODO: REMOVE FALSE
				AddEmitProtocol("Filtered" + protocol, false);
			}
		}

		protected void FilterSignal(string protocol, dynamic signal, List<string> filters)
		{
			filters.ForEach(filter =>
				{
					try
					{
						Expression exp = new Expression(filter);

						// Assign the types in the semantic structure as variables.
						ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);

						st.AllTypes.ForEach(t =>
							{
								exp.Parameters[t.Name] = t.GetValue(rsys.SemanticTypeSystem, signal);
							});

						// Allow parsing of additional functions.
						exp.EvaluateFunction += OnEvaluateFunction;

						object result = exp.Evaluate();

						if (result is bool)
						{
							if ((bool)result)
							{
								// Copy the input signal to the Filtered[protocol] signal for emission.
								CreateCarrier("Filtered" + protocol, outSignal =>
									{
										st.AllTypes.ForEach(t =>
											{
												t.SetValue(rsys.SemanticTypeSystem, outSignal, t.GetValue(rsys.SemanticTypeSystem, signal));
											});
									});
							}
						}
					}
					catch (Exception ex)
					{
						EmitException(ex.Message + " with filter " + filter);
					}
				});
		}

		protected void OnEvaluateFunction(string name, FunctionArgs args)
		{
			if (name.ToLower() == "contains")
			{
				string v1 = args.Parameters[0].Evaluate().ToString().ToLower();
				string v2 = args.Parameters[1].Evaluate().ToString().ToLower();

				args.Result = v1.Contains(v2);
			}
		}

	}
}
