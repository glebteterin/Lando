using System.Collections;
using Lando.Extensions;

namespace Lando
{
	public class LedBuzzerStatus
	{
		public bool FinalRedLedState { get; set; }
		public bool FinalGreenLedState { get; set; }

		public bool UpdateRedLedState { get; set; }
		public bool UpdateGreenLedState { get; set; }

		public bool InitialRedLedBlinkingState { get; set; }
		public bool InitialGreenLedBlinkingState { get; set; }

		public bool RedBlinking { get; set; }
		public bool GreenBlinking { get; set; }

		public HexNumber InitialBlinkingState { get; set; }
		public HexNumber ToggleBlinkingState { get; set; }
		public HexNumber RepetitionNumber { get; set; }

		public BuzzerState Buzzer { get; set; }

		internal byte GetLedState()
		{
			var ledStateBits = new BitArray (
				new []{
					FinalRedLedState, 
					FinalGreenLedState,
					UpdateRedLedState,
					UpdateGreenLedState,
					InitialRedLedBlinkingState,
					InitialGreenLedBlinkingState,
					RedBlinking,
					GreenBlinking}
			);

			return ledStateBits.ToByte();
		}

		internal byte GetT1()
		{
			return InitialBlinkingState.Value;
		}

		internal byte GetT2()
		{
			return ToggleBlinkingState.Value;
		}

		internal byte GetRepetition()
		{
			return RepetitionNumber.Value;
		}

		internal byte GetBuzzerLink()
		{
			switch (Buzzer)
				{
					case BuzzerState.Off: return 0x00;
					case BuzzerState.T1: return 0x01;
					case BuzzerState.T2: return 0x02;
					case BuzzerState.T1T2: return 0x03;
				}

			return 0x00;
		}

		public enum BuzzerState
		{
			Off,
			T1,
			T2,
			T1T2
		}
	}
}