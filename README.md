# Lando
Asynchronous client for ACR122U USB NFC and ACR1222L Readers.<br>
Allows to track statuses of contactless (mifare) cards.

Feature list:

 * Tracks card events (connected, disconnected) associated with a particular cardreader
 * Tracks cardreader events (connected, disconnected)
 * Allows manage let and buzzed by sending APDU commands


#### Usage

```c#
Cardreader reader = new Cardreader();

reader.CardConnected += (sender, args) =>
	Console.WriteLine("Card connected : " + args.Card.Id);

reader.CardDisconnected += (sender, args) =>
	Console.WriteLine("Card Disconnected");

reader.CardreaderConnected += (sender, args) =>
    Console.WriteLine("Cardreader Connected : " + args.CardreaderName);

reader.CardreaderDisconnected += (sender, args) =>
	Console.WriteLine("Cardreader Disconnected : " + args.CardreaderName);
```

#### Models tested:
 * ACR122U
 * ACR1222L