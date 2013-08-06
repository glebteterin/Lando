using Lando.LowLevel.Enums;

namespace Lando.LowLevel
{
	internal class CardState
	{
		public CardStateType CardStateType { get; private set; }

		internal CardState(CardStateType cardState)
		{
			CardStateType = cardState;
		}

		internal CardState(int cardState)
		{
			CardStateType = GetCardState(cardState);
		}

		private CardStateType GetCardState(int cardState)
		{
			var resultStateType = CardStateType.None;

			switch (cardState)
			{
				case WinscardWrapper.SCARD_ABSENT: resultStateType = CardStateType.CardAbsent; break;
				case WinscardWrapper.SCARD_PRESENT: resultStateType = CardStateType.CardPresent; break;
				case WinscardWrapper.SCARD_SWALLOWED: resultStateType = CardStateType.CardSwallowed; break;
				case WinscardWrapper.SCARD_POWERED: resultStateType = CardStateType.CardPowered; break;
				case WinscardWrapper.SCARD_NEGOTIABLE: resultStateType = CardStateType.CardNegotiable; break;
				case WinscardWrapper.SCARD_SPECIFIC: resultStateType = CardStateType.CardSpecific; break;
			}

			return resultStateType;
		}
	}
}