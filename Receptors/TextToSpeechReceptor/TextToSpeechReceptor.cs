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
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace TextToSpeech
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Text To Speech"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "TextToSpeechFilterConfig.xml"; } }

		protected SpeechSynthesizer speechSynth;
		protected Queue<string> speechQueue;

		[UserConfigurableProperty("Protocol Filter:")]
		public string ProtocolFilter { get; set; }

		[UserConfigurableProperty("Show All:")]
		public bool ShowAll { get; set; }

		protected ComboBox cbProtocols;

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			ShowAll = true;

			speechSynth = new SpeechSynthesizer();
			speechSynth.SpeakCompleted += OnSpeakCompleted;
			speechSynth.Rate = -2; // -4;
			speechQueue = new Queue<string>();

			AddReceiveProtocol("Text", (Action<dynamic>)(signal =>
				{
					string msg = signal.Value;
					msg = msg.StripHtml();
					Speak(msg);
				}));

			// Specific protocol for announcements.
			AddReceiveProtocol("Announce", (Action<dynamic>)(signal =>
			{
				string msg = signal.Text.Value;
				msg = msg.StripHtml();
				Speak(msg);
			}));
		}

		public override void PrepopulateConfig(MycroParser mp)
		{
			base.PrepopulateConfig(mp);
			// This combobox is in the config UI.
			cbProtocols = (ComboBox)mp.ObjectCollection["cbProtocols"];
			ReceptorUiHelpers.Helper.PopulateProtocolComboBox(cbProtocols, rsys, ProtocolFilter);
		}

		public override bool UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			ConfigureBasedOnSelectedProtocol();

			return true;
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			if (carrier.Protocol.DeclTypeName == "Text")
			{
				// Filter what protocol we display in the textbox.
				if ((!String.IsNullOrEmpty(ProtocolFilter)) && (!ShowAll))
				{
					if (carrier.ProtocolPath.Contains(ProtocolFilter))
					{
						base.ProcessCarrier(carrier);
					}
				}
				else
				{
					base.ProcessCarrier(carrier);
				}
			}
			else
			{
				base.ProcessCarrier(carrier);
			}
		}

		protected void ConfigureBasedOnSelectedProtocol()
		{
			if (cbProtocols != null)
			{
				// Update the protocol name if the combobox exists, either in the main UI or the configuration UI.
				ProtocolFilter = cbProtocols.SelectedValue.ToString();
			}
		}

		protected void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
		{
			NextSpeech();
		}

		protected void NextSpeech()
		{
			if (speechQueue.Count > 0)
			{
				string msg = speechQueue.Dequeue();
				speechSynth.SpeakAsync(msg);

				// This just doesn't work.  The speechSynth flag does not appear to be very consistent.
				/*
				if (msg.ToLower() == "[pause]")
				{
					// Wait for speech to stop, then pause.
					await Task.Run(() =>
					{
						while (speechSynth.State == SynthesizerState.Speaking)
						{
							Thread.Sleep(100);
						}

						// Thread.Sleep(2000);
					});

					NextSpeech();	// Dequeue next message.
				}
				else
				{
					speechSynth.SpeakAsync(msg);
				}
				 */
			}
		}

		protected void Speak(string msg)
		{
			if (speechSynth.State == SynthesizerState.Speaking)
			{
				// If talking, enqueue the current text.
				speechQueue.Enqueue(msg);
			}
			else
			{
				// If not talking, enqueue anyways, as the flag might not be set yet.
				speechQueue.Enqueue(msg);
				// Start up speech synth.
				NextSpeech();
			}
		}
	}
}
