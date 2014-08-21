using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
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

			AddReceiveProtocol("TextToSpeech");
			AddReceiveProtocol("Text");
		}

		/// <summary>
		/// Handles both "TextToSpeech" and "Text" protocols.
		/// </summary>
		public override void ProcessCarrier(ICarrier carrier)
		{
			string msg = String.Empty; ;

			if (carrier.Protocol.DeclTypeName == "TextToSpeech")
			{
				msg = carrier.Signal.Text;
			}
			else if (carrier.Protocol.DeclTypeName == "Text")
			{
				msg = carrier.Signal.Value;
				msg = msg.StripHtml();
			}

			Speak(msg);
		}

		protected void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
		{
			if (speechQueue.Count > 0)
			{
				string msg = speechQueue.Dequeue();
				speechSynth.SpeakAsync(msg);
			}
		}

		protected void Speak(string msg)
		{
			if (speechSynth.State == SynthesizerState.Speaking)
			{
				speechQueue.Enqueue(msg);
			}
			else
			{
				speechSynth.SpeakAsync(msg);
			}
		}
	}
}


