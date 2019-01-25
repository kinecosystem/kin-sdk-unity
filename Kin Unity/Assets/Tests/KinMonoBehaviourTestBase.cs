using System.Collections;
using System.Collections.Generic;
using Kin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;


public abstract class KinMonoBehaviourTestBase : MonoBehaviour, IMonoBehaviourTest, IPaymentListener, IBalanceListener, IAccountCreationListener
{
	protected KinClient _client;
	protected KinAccount _account;
	protected Transaction _transaction;
	protected PaymentInfo _paymentInfo;
	protected string _whitelist;

	protected string _exportedAccountJson = "{\"pkey\":\"GCG4VVUGX6TPKQ6EVYOFPLBAEYVZS65WQ4MBUBP65OAJWENVGX67UHMF\",\"seed\":\"6a4bc252528c65a9720a1532461175321724ee995ada5759db4b6b3fa505179bd7d1182f5e888ab0ed3c67f69078a6786fbc13da5dd4145bebb6e26df9139aa509e4fe3b7d940eb7\",\"salt\":\"cdedd0db10ba11ff3ec6d41e6b534e91\"}";
	protected string _exportedAccountPassphrase = "43ff735b-b41a-4bdd-9956-79d54e7adcc3";
	protected string _sendToAddress = "GCV7RE24EL2LO2QPONLL4NGTPJUK326ZAWP4NZHCQ5CKE73IWSMM7QXG";
	protected int _feeAmount = 100;

	bool _onAccountCreated;
	bool _onPayment;
	bool _onBalanceChanged;
	
	protected bool _isTestFinished;
	public bool IsTestFinished
	{
		get { return _isTestFinished; }
	}


	void Awake()
	{
		_client = new KinClient( Environment.Test, "test" );
	}


	void OnDestroy()
	{
		_client.ClearAllAccounts();
		_account = null;
	}


	protected void AddAccount()
	{
		_account = _client.AddAccount();
	}


	protected IEnumerator CreateAccount()
	{
		yield return StartCoroutine( KinOnboarding.CreateAccount( _account.GetPublicAddress(), OnCompleteCreateAccount ) );
	}


	protected void ImportActivatedAccount()
	{
		_account = _client.ImportAccount( _exportedAccountJson, _exportedAccountPassphrase );
	}


	protected void AddBlockchainListeners()
	{
		_account.AddAccountCreationListener( this );
		_account.AddBalanceListener( this );
		_account.AddPaymentListener( this );
	}


	protected void ResetBlockchainListeners()
	{
		_onAccountCreated = false;
		_onPayment = false;
		_onBalanceChanged = false;
		_paymentInfo = null;
	}


	protected IEnumerator CheckAccountStatus( AccountStatus statusShouldBe )
	{
        var hasResult = false;
		_account.GetStatus( ( ex, status ) =>
        {
            Assert.IsTrue( status == statusShouldBe );
            hasResult = true;
        } );

        yield return new WaitUntil( () => hasResult );
	}


	protected IEnumerator CheckAccountBalance( decimal balanceShouldBeAtLeast )
	{
        var hasResult = false;
		_account.GetBalance( ( ex, balance ) =>
        {
            Assert.IsTrue( balance > balanceShouldBeAtLeast );
            hasResult = true;
        } );

        yield return new WaitUntil( () => hasResult );
	}


	protected IEnumerator CheckMinimumFee( long feeShouldBeAtLeast )
	{
        var hasResult = false;
		_client.GetMinimumFee( ( ex, fee ) =>
        {
            Assert.IsTrue( fee >= feeShouldBeAtLeast );
            hasResult = true;
        } );

        yield return new WaitUntil( () => hasResult );
	}


	protected IEnumerator BuildTransaction( decimal kinAmount = 100 )
	{
		var hasResult = false;
		_account.BuildTransaction( _sendToAddress, kinAmount, _feeAmount, ( ex, transaction ) =>
		{
			Assert.IsNotNull( transaction );
			Assert.IsNull( ex );
			_transaction = transaction;
			hasResult = true;
		});

		yield return new WaitUntil( () => hasResult );
	}


	protected IEnumerator BuildTransactionWithMemo( decimal kinAmount, string memo )
	{
		var hasResult = false;
		_account.BuildTransaction( _sendToAddress, kinAmount, _feeAmount, memo, ( ex, transaction ) =>
		{
			Assert.IsNotNull( transaction );
			Assert.IsNull( ex );
			_transaction = transaction;
			hasResult = true;
		});

		yield return new WaitUntil( () => hasResult );
	}


	protected IEnumerator WhitelistTransaction()
	{
		yield return StartCoroutine( KinOnboarding.WhitelistTransaction( _transaction, OnCompleteWhitelistTransaction ) );
	}


	protected IEnumerator SendTransaction()
	{
		var hasResult = false;
		_account.SendTransaction( _transaction, ( ex, transactionId ) =>
		{
			Assert.IsNotNull( transactionId );
			Assert.IsNull( ex );
			hasResult = true;
		});

		yield return new WaitUntil( () => hasResult );
	}


	protected IEnumerator SendWhitelistTransaction()
	{
		var hasResult = false;
		_account.SendWhitelistTransaction( _transaction.Id, _whitelist, ( ex, transactionId ) =>
		{
			Assert.IsNotNull( transactionId );
			Assert.IsNull( ex );
			hasResult = true;
		});

		yield return new WaitUntil( () => hasResult );
	}


	void OnCompleteCreateAccount( bool didSucceed )
	{
		Assert.IsTrue( didSucceed );
	}


	void OnCompleteWhitelistTransaction( string whitelist )
	{
		Assert.IsNotNull( whitelist );
		_whitelist = whitelist;
	}

	#region Blockchain Listeners

	protected IEnumerator WaitForAccountCreationListener()
	{
		yield return new WaitUntil( () => _onAccountCreated );
	}


	protected IEnumerator WaitForPaymentListener()
	{
		yield return new WaitUntil( () => _onPayment );
	}


	protected IEnumerator WaitForBalanceChangedListener()
	{
		yield return new WaitUntil( () => _onBalanceChanged );
	}


	public void OnEvent( PaymentInfo payment )
	{
		_paymentInfo = payment;
		_onPayment = true;
	}


	public void OnEvent( decimal balance )
	{
		_onBalanceChanged = true;
	}


	public void OnEvent()
	{
		_onAccountCreated = true;
	}

	#endregion

}
