![Kin Token](kin-banner.jpg)
# Kin SDK for Unity

Kin SDK for Unity is responsible for providing access to the Kin native SDKs for managing Kin balance and transactions.


## Android Setup

The Kin SDK for Unity is a plug-in that uses the Gradle build system on Android. See the [Building with Gradle for Android](https://docs.unity3d.com/Manual/android-gradle-overview.html) section of Unity's documentation and the [Providing a custom build.gradle template](https://docs.unity3d.com/Manual/android-gradle-overview.html) to enable the use of a custom Gradle file.

Open the `Plugins/Android/mainTemplate.gradle` file and add the following:
```gradle
allprojects {
    repositories {
		jcenter()
		google()
		maven { url 'https://jitpack.io' }
    }
}

dependencies {
	implementation 'com.github.kinecosystem:kin-sdk-android:<latest release>'
**DEPS**}

android {
    compileOptions {
        sourceCompatibility 1.8
        targetCompatibility 1.8
    }
}
```
For the latest release version go to [https://github.com/kinecosystem/kin-sdk-android/releases](https://github.com/kinecosystem/kin-sdk-android/releases).

## iOS Setup

In the iOS Player Settings, the `Target minimum iOS Version` must be set to 8.1 or newer.

Note: if you plan on doing any native iOS developement on the plugin, there are some changes that need to be made to the KinSDK, KinUtil and Sodium Xcode projects. They all need to have bitcode enabled and "build active achitectures only" set to no so that you can get debug symobols.

## Getting Started

### Connecting to a service provider

Create a new `KinClient`, with an `Environment` enum that provides details of how to access the Kin Blockchain end points for test and production. `Environment` provides the predefined `Environment.TEST` and `Environment.PRODUCTION` values.

An `appID` is a unique identifier assigned to you by Kin. During testing you can use any four-character string consisting of only upper and/or lower case letters and/or digits. For more information see [appID](https://kinecosystem.github.io/kin-website-docs/docs/appid).


The example below creates a `KinClient` that will be used to connect to the Kin test environment:

```csharp
kinClient = new KinClient( Environment.TEST, <appID> )
```

### Creating and retrieving a Kin account

The first time you use `KinClient` you need to create a new Kin wallet and an associated Kin account. The Kin wallet is stored on the user's client device and holds a public/private key pair. The private key remains securely stored in the local wallet while the public key will become the address of the Kin account added to the Kin Blockchain. Multiple accounts can be created using `AddAccount`.

```csharp
KinAccount account;
try
{
    if( !kinClient.HasAccount() )
        account = kinClient.AddAccount();
}
catch( Exception e )
{
    Debug.LogError( e );
}
```
In the above snippet, if an account does not exist a Wallet and key pair will be created.

Calling `GetAccount` with the existing account index will retrieve the account stored on the device.

```csharp
if( kinClient.HasAccount() )
    account = kinClient.getAccount( 0 );
```

**Warning:** You can delete an account from the device using `deleteAccount`, but beware! The device will lose the account's private key and subsequently will lose access to the Kin stored in the account on the Kin Blockchain.

```csharp
kinClient.DeleteAccount( int index );
```

## Onboarding

At this point in the process your Unity client has created an `account` but the Kin Blockchain does not yet know about it. In the production environment Kin clients are not authorized to create new accounts on the Kin Blockchain. Your server will be authorized to create accounts on their behalf using the [Kin SDK for Python](https://kinecosystem.github.io/kin-website-docs/docs/documentation/python-sdk).  

The process of onboarding a new account consists of two steps, first creating a keypair and `account` structure on the client as we did before, then creating the public address on the Kin Blockchain.

Remember that new accounts are created with 0 Kin, so you will have fund them. On the Kin Blockchain testnet you can create and fund accounts using the [friendbot](https://kinecosystem.github.io/kin-website-docs/docs/friendbot).

## Account Information

### Public Address

Your account can be identified via its public address. To retrieve the account public address use:

```csharp
account.GetPublicAddress();
```

### Query Account Status

Current account status on the blockchain can be queried using the `GetStatus` method. Status will be one of the following 2 options:

* `AccountStatus.NotCreated:` Account is not created on the blockchain. The account cannot send or receive Kin yet.
* `AccountStatus.Created:` Account was created, account can send and receive Kin.

```csharp
account.GetStatus( ( ex, status ) =>
{
	if( ex == null )
		Debug.Log( "Account status: " + status );
	else
		Debug.LogError( "Get Account Status Failed. " + ex );
});
```

### Retrieving Balance

To retrieve the balance of your account in Kin call the `GetBalance` method:

```csharp
account.GetBalance( ( ex, balance ) =>
{
	if( ex == null )
		Debug.Log( "Balance: " + balance );
	else
		Debug.LogError( "Get Balance Failed. " + ex );
});
```

## Transactions

### Transferring Kin to another account

To transfer Kin to another account, you need the public address of the account to which you want to transfer Kin.

By default, your user will need to spend Fee to transfer Kin or process any other blockchain transaction. Fee for individual transactions are trivial (1 KIN = 10<sup>5</sup> FEE).

Some apps can be added to the Kin Whitelist, a set of pre-approved apps whose users will not be charged Fee to execute transactions. If your app is in the  whitelist then refer to [transferring Kin to another account using whitelist service](#transferring-kin-to-another-account-using-whitelist-service).

The snippet [Transfer Kin](#snippet-transfer-kin) will transfer 20 KIN to the recipient account "GDIRGGTBE3H4CUIHNIFZGUECGFQ5MBGIZTPWGUHPIEVOOHFHSCAGMEHO".

###### Snippet: Transfer Kin
```csharp
var toAddress = "GDIRGGTBE3H4CUIHNIFZGUECGFQ5MBGIZTPWGUHPIEVOOHFHSCAGMEHO";
var amountInKin = 20;
var fee = 100;


// we could use here some custom fee or we can can call the blockchain in order to retrieve
// the current minimum fee by calling kinClient.getMinimumFee(). Then when you get the minimum
// fee returned and you can start the 'send transaction flow' with this fee.
account.BuildTransaction( toAddress, amountInKin, fee, ( ex, transaction ) =>
{
	if( ex == null )
	{
        // Here we already got a Transaction object before actually sending the transaction. This means
        // that we can, for example, send the transaction id to our servers or save it locally  
        // in order to use it later. For example if we lose network just after sending
        // the transaction then we will not know what happened with this transaction.
        // So when the network is back we can check what is the status of this transaction.
		Debug.Log( "Build Transaction result: " + transaction );
		account.SendTransaction( transaction, ( ex, transactionId ) =>
		{
			if( ex == null )
				Debug.Log( "Send Transaction result: " + transactionId );
			else
				Debug.LogError( "Send Transaction Failed. " + ex );
		});
	}
	else
	{
		Debug.LogError( "Build Transaction Failed. " + ex );
	}
});
```


### Transferring Kin to another account using Whitelist service

By default, transactions on the Kin Blockchain are charged a minimal fee (1 KIN = 10E5 FEE). Developers can request to be added to a whitelist so their users will not be charged to execute transactions. It is important to submit transactions with the correct methods and parameters to take advantage of this Whitelisting service. Each SDK has specific methods to accomplish this.

If your Unity games are part of the whitelist service, there's an additional step required before your users submit blockchain transactions such as the transfer Kin transaction.

- Send the 'Transaction' object you build in your client to your server running the [Kin SDK for Python](https://kinecosystem.github.io/kin-website-docs/docs/documentation/python-sdk).
- In your server, use the `whitelist_transaction` function in the `KinAccount` class to sign the transaction.
- Send the signed whitelisted transaction back to the client.
- Send the signed whitelisted transaction from the client to the blockchain endpoint.

**Note:** Kin SDKs for clients include methods to prepare transactions for whitelisting. The prepared transactions produced by these methods will include explicit information about the network to which the client wants to send the signed transaction. Because game developers typically avoid creating runtime objects, you can send the unprepared transaction payload to your server but your server will need to supply this network information.



###### Snippet: Whitelist service
```csharp
account.BuildTransaction( toAddress, amountInKin, fee, ( ex, transaction ) =>
{
	if( ex == null )
	{
		Debug.Log( "Build Transaction result: " + transaction );

		var whitelistTransaction = YourWhitelistService.WhitelistTransaction( transaction );
		account.SendWhitelistTransaction( transaction.Id, whitelistTransaction, ( ex, transactionId ) =>
		{
			if( ex == null )
				Debug.Log( "Send Transaction result: " + transactionId );
			else
				Debug.LogError( "Send Transaction Failed. " + ex );
		});
	}
	else
	{
		Debug.LogError( "Build Transaction Failed. " + ex );
	}
});
```


#### Memo

Arbitrary data can be added to a transfer operation using the `memo` parameter containing a UTF-8 string up to 21 bytes in length. A typical usage is to include an order number that a service can use to verify payment.

```csharp
var memo = "arbitrary data";

account.BuildTransaction( toAddress, amountInKin, fee, memo, ( ex, transaction ) =>
{
	if( ex == null )
	{
        // Here we already got a Transaction object before actually sending the transaction. This means
        // that we can, for example, send the transaction id to our servers or save it locally  
        // in order to use it later. For example if we lose network just after sending
        // the transaction then we will not know what happened with this transaction.
        // So when the network is back we can check what is the status of this transaction.
		Debug.Log( "Build Transaction result: " + transaction );
		account.SendTransaction( transaction, ( ex, transactionId ) =>
		{
			if( ex == null )
				Debug.Log( "Send Transaction result: " + transactionId );
			else
				Debug.LogError( "Send Transaction Failed. " + ex );
		});
	}
	else
	{
		Debug.LogError( "Build Transaction Failed. " + ex );
	}
});

account.SendTransaction( toAddress, amountInKin, memo, ( ex, transactionId ) =>
{
	if( ex == null )
		Debug.Log( "Send Transaction: " + transactionId );
	else
		Debug.LogError( "Send Transaction Failed. " + ex );
});
```

## Account Listeners

Your Unity game can respond to payments, balance changes and account creation using listeners.

### Listening to payments

Ongoing payments in Kin, from or to an account, can be observed with a payment listener:

```csharp
account.AddPaymentListener( this );

...

public void OnEvent( PaymentInfo payment )
{
	Debug.Log( "On Payment: " + payment );
}
```

### Listening to balance changes

Account balance changes can be observed with a balance listener:

```csharp
account.AddBalanceListener( this );

...

public void OnEvent( decimal balance )
{
	Debug.Log( "On Balance: " + balance );
}
```

### Listening to account creation

Account creation on the blockchain network can be observed by adding and account creation listener:

```csharp
account.AddAccountCreationListener( this );

...

public void OnEvent()
{
	Debug.Log( "On Account Created" );
}
```

To unregister any listener use `RemovePaymentListener`, `RemoveBalanceListener` or `RemoveAccountCreationListener` methods.


## Error Handling

The Kin SDK Unity Plugin wraps Kin native exceptions in the C# KinException class. It provides a `NativeType` field that will contain the native error type, some of which are in the Common Error section that follows.


### Common Errors

`AccountNotFoundException` - Account is not created (funded with native asset) on the network.  
`AccountNotActivatedException` - Account was created but not activated yet, the account cannot send or receive Kin yet.  
`InsufficientKinException` - Account has not enough kin funds to perform the transaction.


## Demo Scene

The demo scene included with the Kin Unity Plugin covers the functionality of the plugin, and serves as a detailed example on how to use it.



## Contributing
Please review our [CONTRIBUTING.md](CONTRIBUTING.md) guide before opening issues and pull requests.


## License
The kin-unity-plugin is licensed under [MIT license](LICENSE.md).
