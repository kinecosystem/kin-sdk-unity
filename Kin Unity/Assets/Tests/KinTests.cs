using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using Kin;
using UnityEngine.Assertions;


public class KinTests
{
	KinClient _client;
	KinAccount _account;
	string _exportPassphrase = "sdaf";


	[NUnit.Framework.SetUp]
	public void Setup()
	{
		_client = new KinClient( Environment.Test, "test" );
		_client.ClearAllAccounts();
	}


	[NUnit.Framework.TearDown]
	public void TearDown()
	{
		_client.ClearAllAccounts();
	}


	[NUnit.Framework.Test]
	public void GetAccountShouldNotExist()
	{
		var account = _client.GetAccount();
		Assert.IsNull( account );
	}


	[NUnit.Framework.Test]
	public void AddAccountShouldAddAccount()
	{
		var account = _client.AddAccount();
		Assert.IsNotNull( account );
		Assert.IsTrue( _client.GetAccountCount() == 1 );
	}


	[NUnit.Framework.Test]
	public void GetBalanceWithNotCreatedAccountShouldFail()
	{
		var account = _client.AddAccount();
		account.GetBalance( ( ex, balance ) =>
		{
			Assert.IsNotNull( ex );
		});
	}


	[NUnit.Framework.Test]
	public void DeleteAccountShouldDeleteAddedAccount()
	{
		_client.AddAccount();
		_client.DeleteAccount();
		Assert.IsTrue( _client.GetAccountCount() == 0 );
	}


	[NUnit.Framework.Test]
	public void ExportAccountShouldExportAccount()
	{
		var account = _client.AddAccount();
		Assert.IsNotNull( account );

		var json = account.Export( _exportPassphrase );
		Assert.IsTrue( _client.GetAccountCount() == 1 );
        Assert.IsTrue( json.Length > 0 );
	}


	[NUnit.Framework.Test]
	public void ImportAccountShouldImportAccount()
	{
		var account = _client.AddAccount();
		Assert.IsNotNull( account );

		var json = account.Export( _exportPassphrase );
		Assert.IsTrue( _client.GetAccountCount() == 1 );
		_client.ClearAllAccounts();

		account = _client.ImportAccount( json, _exportPassphrase );
		Assert.IsNotNull( account );
		Assert.IsTrue( _client.GetAccountCount() == 1 );
	}


	[UnityTest]
	public IEnumerator AccountStatusShouldBeNotCreated()
	{
		var account = _client.AddAccount();
        var hasResult = false;
		account.GetStatus( ( ex, status ) =>
        {
            Assert.IsTrue( status == AccountStatus.NotCreated );
            hasResult = true;
        } );

        yield return new WaitUntil( () => hasResult );
	}


	[UnityTest]
	public IEnumerator BuildTransactionWithNotCreatedAccountShouldFail()
	{
		var account = _client.AddAccount();
        var hasResult = false;
		account.BuildTransaction( "to-address", 100, 100, ( ex, transaction ) =>
		{
			Assert.IsNotNull( ex );
			Assert.IsNull( transaction );
			hasResult = true;
		});

        yield return new WaitUntil( () => hasResult );
	}


	[UnityTest]
	public IEnumerator AddAndCreateAccount()
	{
		yield return new MonoBehaviourTest<AddAndCreateAccountTest>();
	}


	[UnityTest]
	public IEnumerator SendTransactionTest()
	{
		yield return new MonoBehaviourTest<SendTransactionTest>();
	}


	[UnityTest]
	public IEnumerator SendWhitelistTransactionTest()
	{
		yield return new MonoBehaviourTest<SendWhitelistTransactionTest>();
	}


	[UnityTest]
	public IEnumerator BlockchainListenerTest()
	{
		yield return new MonoBehaviourTest<BlockchainListenerTest>();
	}


	// [UnityTest]
	// [NUnit.Framework.Timeout( int.MaxValue )]
	// public IEnumerator TransactionVerificationTest()
	// {
	// 	yield return new MonoBehaviourTest<TransactionVerificationTest>();
	// }

}
