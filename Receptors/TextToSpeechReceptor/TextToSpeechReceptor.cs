using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace TextToSpeech
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Text To Speech"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		protected SpeechSynthesizer speechSynth;
		protected Queue<string> speechQueue;

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
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
