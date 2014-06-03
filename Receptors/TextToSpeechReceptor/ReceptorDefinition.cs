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
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Text To Speech"; } }
		public bool IsEdgeReceptor { get { return true; } }
		public bool IsHidden { get { return false; } }

		protected SpeechSynthesizer speechSynth;
		protected Queue<string> speechQueue;
		protected IReceptorSystem rsys;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;

			speechSynth = new SpeechSynthesizer();
			speechSynth.SpeakCompleted += OnSpeakCompleted;
			speechSynth.Rate = -4;
			speechQueue = new Queue<string>();
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "TextToSpeech", "Text"};
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { };
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		/// <summary>
		/// Handles both "TextToSpeech" and "Text" protocols.
		/// </summary>
		public void ProcessCarrier(ICarrier carrier)
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


