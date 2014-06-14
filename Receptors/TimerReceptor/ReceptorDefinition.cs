using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;				// So we can get a timer marshalled on the UI thread.

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

using Clifton.Tools.Strings.Extensions;

namespace TimerReceptor
{
	internal class IntervalTimer
	{
#pragma warning disable 67
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;
#pragma warning restore 67

		public DateTime? StartDateTime { get; set; }
		public int Interval { get; set; }
		public string EventName { get; set; }
		public bool IgnoreMissedIntervals { get; set; }
		public bool Running { get { return running; } }
		public bool PreExisting { get; set; }

		public IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;
		protected IReceptorInstance receptor;

		// Internal, to track last time event was fired.
		public DateTime? LastEventTime { get; set; }

		protected bool running;
		protected DateTime nextEvent;

		public IntervalTimer(IReceptorSystem rsys, IReceptorInstance receptor)
		{
			this.rsys = rsys;
			this.receptor = receptor;
		}

		// If there is no last event time, then we fire events either:
		// 1 - from the start date/time to now
		// 2 - a single event and set the last event time to the point where the event would have fired.
		public void FireIfExpired()
		{
			// May be in the database, but no configuration information has been received to tell us we actually want this timer.
			if (StartDateTime != null)
			{
				DateTime startDateTime = (DateTime)StartDateTime;
				DateTime now = DateTime.Now;

				if (!running)
				{
					if (LastEventTime == null)
					{
						// There is no previously record last event.
						if (IgnoreMissedIntervals)
						{
							// Catch up, so that the next event is at a multiple of the interval after "now."
							TimeSpan ts = now - startDateTime;
							long mostRecentEvent = (long)ts.TotalSeconds / (long)Interval;
							// Remainder has been removed.
							LastEventTime = startDateTime.AddSeconds(mostRecentEvent * (long)Interval);
							// This is the last event that should have fired.
							FireEvent((DateTime)LastEventTime);
							// This is the next event time.
							nextEvent = ((DateTime)LastEventTime).AddSeconds(Interval);
						}
						else
						{
							// Fire for all missed events.
							nextEvent = (DateTime)StartDateTime;

							while (nextEvent < now)
							{
								FireEvent(nextEvent);
								nextEvent = nextEvent.AddSeconds(Interval);
							}
						}
					}
					else
					{
						if (IgnoreMissedIntervals)
						{
							// Catch up, so that the next event is at a multiple of the interval after "now."
							TimeSpan ts = now - (DateTime)LastEventTime;
							long mostRecentEvent = (long)ts.TotalSeconds / (long)Interval;
							// Remainder has been removed.
							LastEventTime = startDateTime.AddSeconds(mostRecentEvent * (long)Interval);
							// This is the last event that should have fired.
							FireEvent((DateTime)LastEventTime);
							// This is the next event time.
							nextEvent = ((DateTime)LastEventTime).AddSeconds(Interval);
						}
						else
						{
							// Fire for all missed events.
							nextEvent = ((DateTime)LastEventTime).AddSeconds(Interval);

							while (nextEvent < now)
							{
								FireEvent(nextEvent);
								nextEvent = nextEvent.AddSeconds(Interval);
							}
						}
					}

					running = true;
				}
				else
				{
					// A running event timer will have compensated for missed events.  
					if (now >= nextEvent)
					{
						FireEvent(nextEvent);
						nextEvent = nextEvent.AddSeconds(Interval);
					}
				}
			}
		}

		/// <summary>
		/// Fire an event NOW.
		/// </summary>
		protected void FireEvent(DateTime when)
		{
			LastEventTime = when;
			UpdateRecord();
			CreateEventCarrier();
		}

		/// <summary>
		/// Updates an existing record with the new "LastEventTime"
		/// or inserts a new record if it didn't already exist.
		/// </summary>
		protected void UpdateRecord()
		{
			ICarrier rowCarrier = CreateRow();

			// If it already exists in the DB, update it
			if (PreExisting)
			{
				UpdateRecord(rowCarrier);
			}
			else
			{
				// Otherwise, create it
				PreExisting = true;
				InsertRecord(rowCarrier);
			}
		}

		/// <summary>
		/// Creates the carrier for the timer event.
		/// </summary>
		protected void CreateEventCarrier()
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("TimerEvent");
			dynamic signal = rsys.SemanticTypeSystem.Create("TimerEvent");
			signal.EventName = EventName;
			signal.EventDateTime = (DateTime)LastEventTime;
			rsys.CreateCarrier(receptor, protocol, signal);
		}

		/// <summary>
		/// Creates the carrier (as an internal carrier, not exposed to the system) for containing
		/// our record information.  Assuming only an update, this sets only the EventDateTime field.
		/// </summary>
		protected ICarrier CreateRow()
		{
			// Create the type for the updated data.
			ISemanticTypeStruct rowProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("LastEventDateTime");
			dynamic rowSignal = rsys.SemanticTypeSystem.Create("LastEventDateTime");
			rowSignal.EventDateTime = LastEventTime;
			ICarrier rowCarrier = rsys.CreateInternalCarrier(rowProtocol, rowSignal);

			return rowCarrier;
		}

		/// <summary>
		/// Creates a carrier instructing the persistor to update the LastEventDateTime field for our event name.
		/// </summary>
		protected void UpdateRecord(ICarrier rowCarrier)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DatabaseRecord");
			dynamic signal = rsys.SemanticTypeSystem.Create("DatabaseRecord");
			signal.TableName = "LastEventDateTime";
			signal.Row = rowCarrier;
			signal.Action = "update";
			signal.Where = "EventName = " + EventName.SingleQuote();
			rsys.CreateCarrier(receptor, protocol, signal);
		}

		/// <summary>
		/// Creates a carrier instructing the persistor to create a new entry.  Note that we also
		/// add the EventName to the field set.
		/// </summary>
		protected void InsertRecord(ICarrier rowCarrier)
		{
			rowCarrier.Signal.EventName = EventName;
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DatabaseRecord");
			dynamic signal = rsys.SemanticTypeSystem.Create("DatabaseRecord");
			signal.TableName = "LastEventDateTime";
			signal.Row = rowCarrier;
			signal.Action = "insert";
			rsys.CreateCarrier(receptor, protocol, signal);
		}
	}

	public class ReceptorDefinition : IReceptorInstance
	{
#pragma warning disable 67
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;
#pragma warning restore 67

		public string Name { get { return "Interval Timer"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		public IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;
		protected Dictionary<string, Action<dynamic>> protocolActionMap;
		protected bool ready;
		protected Timer timer;

		private Dictionary<string, IntervalTimer> intervalMap;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			intervalMap = new Dictionary<string, IntervalTimer>();

			protocolActionMap = new Dictionary<string, Action<dynamic>>();
			protocolActionMap["IntervalTimerConfiguration"] = new Action<dynamic>((s) => IntervalTimerConfiguration(s));
			protocolActionMap["LastEventDateTimeRecordset"] = new Action<dynamic>((s) => LastEventDateTimeRecordset(s));
		}

		public string[] GetReceiveProtocols()
		{
			return protocolActionMap.Keys.ToArray();
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { "RequireTable", "DatabaseRecord" };
		}

		/// <summary>
		/// Post-creation initialization.
		/// </summary>
		public void Initialize()
		{
			RequireEventTable();
			GetLastKnownEvents();
			InitializeTimer();
		}

		public void Terminate()
		{
			timer.Stop();
			timer.Dispose();
		}

		public void ProcessCarrier(ICarrier carrier)
		{
			protocolActionMap[carrier.Protocol.DeclTypeName](carrier.Signal);
		}

		protected void InitializeTimer()
		{
			timer = new Timer();
			timer.Interval = 1000;
			timer.Tick += FireEvents;
			timer.Start();
		}

		/// <summary>
		/// Process a configuration carrier, which will add a new timer event.
		/// </summary>
		/// <param name="signal"></param>
		protected void IntervalTimerConfiguration(dynamic signal)
		{
			string eventName = signal.EventName;

			// If the interval timer does not exist, create it now.
			if (!intervalMap.ContainsKey(eventName))
			{
				IntervalTimer interval = new IntervalTimer(rsys, this)
				{
					StartDateTime = signal.StartDateTime,
					Interval = signal.Interval,
					EventName = signal.EventName,
					IgnoreMissedIntervals = signal.IgnoreMissedIntervals,
					PreExisting = false
				};
				intervalMap[eventName] = interval;
			}
			else
			{
				// We already have an interval timer for this event.  Fill in the missing pieces.
				intervalMap[eventName].StartDateTime = signal.StartDateTime;
				intervalMap[eventName].Interval = signal.Interval;
				intervalMap[eventName].IgnoreMissedIntervals = signal.IgnoreMissedIntervals;
			}

			FireExpiredEvents();
		}

		/// <summary>
		/// Process the return from the store, getting a the last known time an event fired.
		/// </summary>
		protected void LastEventDateTimeRecordset(dynamic signalRecordset)
		{
			foreach (dynamic signal in signalRecordset.Recordset)
			{
				string eventName = signal.EventName;
				DateTime eventDateTime = signal.EventDateTime;

				// If the interval timer doesn't exist yet, then create a placeholder.
				if (!intervalMap.ContainsKey(eventName))
				{
					intervalMap[eventName] = new IntervalTimer(rsys, this) { EventName = eventName, LastEventTime = eventDateTime, PreExisting = true };
				}
				else
				{
					// If it does exist but it's not running yet, update the last event time.
					if (!intervalMap[eventName].Running)
					{
						intervalMap[eventName].LastEventTime = eventDateTime;
						intervalMap[eventName].PreExisting = true;
					}
				}
			}

			// Now that we know where we're at, we can fire expired events.
			FireExpiredEvents();
			ready = true;
		}

		protected void FireEvents(object sender, EventArgs args)
		{
			FireExpiredEvents();
		}

		protected void FireExpiredEvents()
		{
			// If we've gotten a response from the store, we can go ahead and start processing events.
			if (ready)
			{
				intervalMap.Values.ForEach(t => t.FireIfExpired());
			}
		}

		/// <summary>
		/// We require this table from the persistor.
		/// </summary>
		protected void RequireEventTable()
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("RequireTable");
			dynamic signal = rsys.SemanticTypeSystem.Create("RequireTable");
			signal.TableName = "LastEventDateTime";
			signal.Schema = "LastEventDateTime";
			rsys.CreateCarrier(this, protocol, signal);
		}

		/// <summary>
		/// Get the known events from the store
		/// </summary>
		protected void GetLastKnownEvents()
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DatabaseRecord");
			dynamic signal = rsys.SemanticTypeSystem.Create("DatabaseRecord");
			signal.ResponseProtocol = "LastEventDateTime";
			signal.TableName = "LastEventDateTime";
			signal.Action = "select";
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}


