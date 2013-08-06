using System;
using Lando.LowLevel;
using NUnit.Framework;

namespace Lando.UnitTests.Integration
{
	[TestFixture]
	[Explicit]
	public class LowLevelIntegrationalTests
	{
		readonly LowLevelCardReader _reader = new LowLevelCardReader();

		private bool _isContextEstablished;

		[SetUp]
		public void Setup()
		{
			var fact = _reader.EstablishContext();

			_isContextEstablished = fact.IsSuccessful;

			if(!_isContextEstablished)
				throw new Exception("Context is not established");
		}

		[Test]
		public void EstablishContext()
		{
			Assert.That(_isContextEstablished, Is.True);
		}

		[Test]
		public void GetCardReadersList()
		{
			string[] readerNames;

			// act
			var getCardReadersResult = GetCardreaderNames(out readerNames);

			Assert.That(getCardReadersResult, Is.True);
			Assert.That(readerNames.Length, Is.EqualTo(1));
		}

		private bool GetCardreaderNames(out string[] result)
		{
			var getCardReadersResult = _reader.GetCardReadersList(out result);
			return getCardReadersResult.IsSuccessful;
		}
	}
}