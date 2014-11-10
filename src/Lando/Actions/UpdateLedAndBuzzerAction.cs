using System;
using Lando.LowLevel;

namespace Lando.Actions
{
	internal class UpdateLedAndBuzzerAction : ICardreaderAction
	{
		public CardreaderActionType Type { get { return CardreaderActionType.UpdateLedAndBuzzer; } }

		public ContactlessCard Card { get; private set; }
		public LedBuzzerStatus Status { get; private set; }

		public UpdateLedAndBuzzerAction(ContactlessCard card, LedBuzzerStatus status)
		{
			Card = card;
			Status = status;
		}

		public void Execute(LowLevelCardReader lowLevelCardReader)
		{
			if (lowLevelCardReader == null) throw new ArgumentNullException("lowLevelCardReader");

			lowLevelCardReader.UpdateLedAndBuzzer(
				Card.Card, Status.GetLedState(),
				Status.GetT1(), Status.GetT2(),
				Status.GetRepetition(),
				Status.GetBuzzerLink());
		}
	}
}