using System;
using System.Threading;
using NUnit.Framework;

namespace Lando.UnitTests.Integration
{
	[TestFixture]
	[Explicit]
	public class HighLevelIntegrationTests
	{
		private readonly Cardreader _reader = new Cardreader();

		private int _flag = 0;
		private bool _buzzerWasSetup = false;

		[Test]
		public void Test()
		{
			_reader.CardConnected += (sender, args) =>
			{
				SetRed(args.Card);
				Thread.Sleep(1000);

				Console.WriteLine("Card connected : " + args.Card.Id);

				if (!_buzzerWasSetup)
				{
					_reader.SetBuzzerOutputForCardDetection(args.Card, false);
					_buzzerWasSetup = true;
				}

				switch (_flag)
				{
					case 0: SetGreenAndShortBeep(args.Card); _flag = 1; break;
					case 1: SetGreenAndShortBeepTwice(args.Card); _flag = 2; break;
					case 2: SetRedAndLongBeep(args.Card); _flag = 0; break;
				}
			};
			_reader.CardDisconnected += (sender, args) => Console.WriteLine("Card Disconnected");
			_reader.CardreaderConnected += (sender, args) => Console.WriteLine("Cardreader Connected : " + args.CardreaderName);
			_reader.CardreaderDisconnected += (sender, args) => Console.WriteLine("Cardreader Disconnected : " + args.CardreaderName);
			_reader.Error += (sender, args) => Console.WriteLine("Error : " + args.CardreaderName);

			_reader.StartWatch();

			Thread.Sleep(10 * 60 * 1000);

			_reader.StopWatch();
		}

		private void SetRed(ContactlessCard card)
		{
			var status = new LedBuzzerStatus();
			status.FinalRedLedState = true;
			status.FinalGreenLedState = false;
			status.UpdateRedLedState = true;
			status.UpdateGreenLedState = true;
			status.InitialRedLedBlinkingState = true;
			status.InitialGreenLedBlinkingState = true;
			status.RedBlinking = false;
			status.GreenBlinking = false;
			status.InitialBlinkingState = HexNumber.One;
			status.ToggleBlinkingState = HexNumber.Zero;
			status.RepetitionNumber = HexNumber.One;
			status.Buzzer = LedBuzzerStatus.BuzzerState.Off;

			_reader.UpdateLedAndBuzzer(card, status);
		}

		private void SetRedAndLongBeep(ContactlessCard card)
		{
			var status = new LedBuzzerStatus();
			status.FinalRedLedState = true;
			status.FinalGreenLedState = false;
			status.UpdateRedLedState = true;
			status.UpdateGreenLedState = true;
			status.InitialRedLedBlinkingState = true;
			status.InitialGreenLedBlinkingState = false;
			status.RedBlinking = false;
			status.GreenBlinking = false;
			status.InitialBlinkingState = HexNumber.Ten;
			status.ToggleBlinkingState = HexNumber.Zero;
			status.RepetitionNumber = HexNumber.One;
			status.Buzzer = LedBuzzerStatus.BuzzerState.T1;

			_reader.UpdateLedAndBuzzer(card, status);
		}

		private void SetGreenAndShortBeep(ContactlessCard card)
		{
			var status = new LedBuzzerStatus();
			status.FinalRedLedState = false;
			status.FinalGreenLedState = false;
			status.UpdateRedLedState = true;
			status.UpdateGreenLedState = true;
			status.InitialRedLedBlinkingState = false;
			status.InitialGreenLedBlinkingState = true;
			status.RedBlinking = false;
			status.GreenBlinking = false;
			status.InitialBlinkingState = HexNumber.One;
			status.ToggleBlinkingState = HexNumber.Zero;
			status.RepetitionNumber = HexNumber.One;
			status.Buzzer = LedBuzzerStatus.BuzzerState.T1;

			_reader.UpdateLedAndBuzzer(card, status);
		}

		private void SetGreenAndShortBeepTwice(ContactlessCard card)
		{
			var status = new LedBuzzerStatus();
			status.FinalRedLedState = false;
			status.FinalGreenLedState = false;
			status.UpdateRedLedState = true;
			status.UpdateGreenLedState = true;
			status.InitialRedLedBlinkingState = false;
			status.InitialGreenLedBlinkingState = true;
			status.RedBlinking = false;
			status.GreenBlinking = false;
			status.InitialBlinkingState = HexNumber.One;
			status.ToggleBlinkingState = HexNumber.One;
			status.RepetitionNumber = HexNumber.Two;
			status.Buzzer = LedBuzzerStatus.BuzzerState.T1;

			_reader.UpdateLedAndBuzzer(card, status);
		}
	}
}