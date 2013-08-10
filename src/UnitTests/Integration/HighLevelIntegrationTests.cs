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

		[Test]
		public void Test()
		{
			_reader.CardConnected += (sender, args) => Console.WriteLine("Card connected : " + args.Card.Id);
			_reader.CardDisconnected += (sender, args) => Console.WriteLine("Card Disconnected");
			_reader.CardreaderConnected += (sender, args) => Console.WriteLine("Cardreader Connected : " + args.CardreaderName);
			_reader.CardreaderDisconnected += (sender, args) => Console.WriteLine("Cardreader Disconnected : " + args.CardreaderName);
			_reader.Error += (sender, args) => Console.WriteLine("Error : " + args.CardreaderName);

			_reader.StartWatch();

			Thread.Sleep(10 * 60 * 1000);

			_reader.StopWatch();
		}
	}
}