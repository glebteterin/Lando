using System;
using Lando.LowLevel;
using Lando.LowLevel.Enums;
using NUnit.Framework;

namespace Lando.UnitTests.Integration
{
	[TestFixture]
	[Explicit]
	public class LowLevelIntegrationalTests
	{
		readonly LowLevelCardReader _reader = new LowLevelCardReader();

		private bool _isContextEstablished;
		private Card _card;

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

		[Test]
		public void Connect()
		{
			// act
			var connectResult = _reader.Connect(GetCardreaderName());

			Assert.That(connectResult.IsSuccessful, Is.True);
		}

		[Test]
		public void GetCardState()
		{
			// arrange
			_card = _reader.Connect(GetCardreaderName()).ConnectedCard;

			// act
			var stateResult = _reader.GetCardState(_card);

			Assert.That(stateResult, Is.True);
			Assert.That(_card.State.CardStateType, Is.EqualTo(CardStateType.CardSpecific));
		}

		[Test]
		public void GetCardId()
		{
			// arrange
			_card = _reader.Connect(GetCardreaderName()).ConnectedCard;

			// act
			var getCardIdResult = _reader.GetCardId(_card);

			Assert.That(getCardIdResult.Bytes.Length, Is.GreaterThan(0));
		}

		[Test]
		public void Led()
		{
			// arrange
			if (_card == null)
				_card = _reader.Connect(GetCardreaderName()).ConnectedCard;

			// act
			var getCardIdResult = _reader.GetCardId(_card);
			_reader.UpdateLedAndBuzzer(_card, 0x00, 0x00, 0x00, 0x00, 0x00);

			Assert.That(getCardIdResult.Bytes.Length, Is.GreaterThan(0));
		}

		[Test]
		public void DisableDefaultBuzzer()
		{
			// arrange
			if (_card == null)
				_card = _reader.Connect(GetCardreaderName()).ConnectedCard;

			// act
			var operationResult = _reader.SetBuzzerOutputForCardDetection(_card, false);

			Assert.That(operationResult, Is.EqualTo(OperationResultType.Success));
		}

		private string GetCardreaderName()
		{
			string[] readerNames;

			GetCardreaderNames(out readerNames);

			return readerNames[0];
		}

		private bool GetCardreaderNames(out string[] result)
		{
			var getCardReadersResult = _reader.GetCardReadersList(out result);
			return getCardReadersResult.IsSuccessful;
		}
	}
}