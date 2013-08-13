using System;
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

			// on ubuntu the result of SCardStatus for presented card is newer equals 2 and always different
			// so I consider any unexpected value as CardPresent except 1-999 - it's a system error in Windows

			if (resultStateType == CardStateType.None && cardState > 999)
			{
				if (cardState > 999)
					resultStateType = CardStateType.CardPresent;
				else
					throw new Exception("Supplied parameter is out of scope of WinscardWrapper.");
			}

			return resultStateType;
		}
	}
}