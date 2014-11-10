using System;
using Lando.LowLevel;

namespace Lando.Actions
{
	internal class SetBuzzerOutputForCardDetectionAction : ICardreaderAction
	{
		public CardreaderActionType Type { get { return CardreaderActionType.SetBuzzerOutputForCardDetection; } }

		public ContactlessCard Card { get; private set; }
		public bool ShouldBuzzWhenCardDetected { get; private set; }

		public SetBuzzerOutputForCardDetectionAction(ContactlessCard card, bool shouldBuzzWhenCardDetected)
		{
			Card = card;
			ShouldBuzzWhenCardDetected = shouldBuzzWhenCardDetected;
		}

		public void Execute(LowLevelCardReader lowLevelCardReader)
		{
			if (lowLevelCardReader == null) throw new ArgumentNullException("lowLevelCardReader");

			lowLevelCardReader.SetBuzzerOutputForCardDetection(Card.Card, ShouldBuzzWhenCardDetected);
		}
	}
}