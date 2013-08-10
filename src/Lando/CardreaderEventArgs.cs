namespace Lando
{
	public class CardreaderEventArgs
	{
		public ContactlessCard Card { get; set; }

		public string CardreaderName { get; set; }

		public CardreaderEventArgs(ContactlessCard card)
		{
			Card = card;
		}

		public CardreaderEventArgs(string cardreaderName)
		{
			CardreaderName = cardreaderName;
		}

		public CardreaderEventArgs(ContactlessCard card, string cardreaderName)
		{
			Card = card;
			CardreaderName = cardreaderName;
		}
	}
}