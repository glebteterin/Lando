using System;
using Lando.LowLevel;

namespace Lando
{
	public class ContactlessCard
	{
		internal Card Card { get; private set; }

		internal string CardreaderName { get; private set; }

		public string Id { get; private set; }

		internal ContactlessCard(Card card)
		{
			Id = BitConverter.ToString(card.IdBytes);

			Card = card;

			CardreaderName = card.CardreaderName;
		}
	}
}