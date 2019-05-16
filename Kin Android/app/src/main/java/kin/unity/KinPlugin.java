package kin.unity;


import android.text.TextUtils;
import android.util.Log;

import java.math.BigDecimal;
import java.util.HashMap;

import kin.sdk.Balance;
import kin.sdk.KinAccount;
import kin.sdk.KinClient;
import kin.sdk.ListenerRegistration;
import kin.sdk.Transaction;
import kin.sdk.TransactionId;
import kin.sdk.exception.DeleteAccountException;
import kin.sdk.exception.CryptoException;
import kin.sdk.Environment;


@SuppressWarnings("unused")
public class KinPlugin extends KinPluginBase
{
	private static final KinPlugin _instance = new KinPlugin();

	// cache for our native classes that are mirrored in C#
	private HashMap<String,KinClient> _clients = new HashMap<>();
	private HashMap<String,KinAccount> _accounts = new HashMap<>();
	private HashMap<String,Transaction> _transactions = new HashMap<>();

	// ListenerRegistration cache
	private HashMap<String,ListenerRegistration> _paymentListeners = new HashMap<>();
	private HashMap<String,ListenerRegistration> _balanceListeners = new HashMap<>();
	private HashMap<String,ListenerRegistration> _accountListeners = new HashMap<>();


	public static KinPlugin instance()
	{
		return _instance;
	}


	//region KinClient

	public void createClient( String clientId, int environment, String appId, String storeKey )
	{
		// KinClient defaults to null so we do as well here
		if( storeKey == null )
			storeKey = "";

		Environment env = environment == 0 ? Environment.TEST : Environment.PRODUCTION;
		KinClient client = new KinClient( getActivity(), env, appId, storeKey );
		_clients.put( clientId, client );
	}


	public KinClient _getClient( String clientId )
	{
		return _clients.get( clientId );
	}


	public void freeCachedClient( String clientId )
	{
		if( _clients.containsKey( clientId ) )
		{
			Log.i( TAG, "freeing cached client: " + clientId );
			_clients.remove( clientId );
		}
	}


	public String importAccount( String clientId, String accountId, String exportedJson, String passphrase )
	{
		try
		{
			KinAccount account = _clients.get( clientId ).importAccount( exportedJson, passphrase );
			_accounts.put( accountId, account );
			return "";
		}
		catch( Exception e )
		{
			e.printStackTrace();
			return exceptionToJson( e, null );
		}
	}


	public int getAccountCount( String clientId )
	{
		return _clients.get( clientId ).getAccountCount();
	}


	public String addAccount( String clientId, String accountId )
	{
		try
		{
			Log.i( TAG, "adding account: " + accountId );
			KinAccount account = _clients.get( clientId ).addAccount();
			Log.i( TAG, "added account successfully" );
			_accounts.put( accountId, account );
		}
		catch( Exception e )
		{
			e.printStackTrace();
			return exceptionToJson( e, null );
		}

		return "";
	}


	public boolean getAccount( String clientId, String accountId, int index )
	{
		KinAccount account = _clients.get( clientId ).getAccount( index );
		if( account == null )
			return false;

		if( !_accounts.containsValue( account ) )
			_accounts.put( accountId, account );

		return true;
	}


	public KinAccount _getRawAccount( String clientId, int index )
	{
		return _clients.get( clientId ).getAccount( index );
	}


	public String deleteAccount( String clientId, int index )
	{
		Log.i( TAG, "deleting account: " + index );
		KinAccount account = _clients.get( clientId ).getAccount( index );
		if( account == null )
		{
			Log.i( TAG, "could not find account to delete" );
			String message = "Attempted to delete account that doesn't exist at index " + index;
			Log.e( TAG, message );
			return exceptionToJson( new IndexOutOfBoundsException( message ), null );
		}

		try
		{
			// attempt to delete the account
			_clients.get( clientId ).deleteAccount( index );
		}
		catch( Exception e )
		{
			e.printStackTrace();
			return exceptionToJson( e, null );
		}

		return "";
	}


	public void clearAllAccounts( String clientId )
	{
		try
		{
			_clients.get( clientId ).clearAllAccounts();
		}
		catch( Exception e )
		{
			e.printStackTrace();
		}
	}

	//endregion


	//region KinAccount

	public void freeCachedAccount( final String accountId )
	{
		if( _accounts.containsKey( accountId ) )
		{
			Log.i( TAG, "freeing cached account: " + accountId );
			_accounts.remove( accountId );
		}
	}


	public String getPublicAddress( final String accountId )
	{
		return _accounts.get( accountId ).getPublicAddress();
	}


	public String export( final String accountId, final String passphrase )
	{
		try
		{
			return _accounts.get( accountId ).export( passphrase );
		}
		catch( Exception e )
		{
			e.printStackTrace();
			return exceptionToJson( e, null );
		}
	}


	public void getStatus( final String accountId )
	{
		new Thread( () -> {
			try
			{
				int status = _accounts.get( accountId ).getStatusSync();
				unitySendMessage( "GetStatusSucceeded", callbackToJson( String.valueOf( status ), accountId ) );
			}
			catch( Exception e )
			{
				unitySendMessage( "GetStatusFailed", exceptionToJson( e, accountId ) );
				Log.e( TAG, "GetStatus failed", e );
			}
		} ).start();
	}


	public void getBalance( final String accountId )
	{
		new Thread( () -> {
			try
			{
				Balance balance = _accounts.get( accountId ).getBalanceSync();
				unitySendMessage( "GetBalanceSucceeded", callbackToJson( balance.value().toString(), accountId ) );
			}
			catch( Exception e )
			{
				unitySendMessage( "GetBalanceFailed", exceptionToJson( e, accountId ) );
				Log.e( TAG, "GetBalance failed", e );
			}
		} ).start();
	}


	public void getMinimumFee( final String clientId )
	{
		new Thread( () -> {
			try
			{
				long fee = _clients.get( clientId ).getMinimumFeeSync();
				unitySendMessage( "GetMinimumFeeSucceeded", callbackToJson( Long.toString( fee ), clientId ) );
			}
			catch( Exception e )
			{
				unitySendMessage( "GetMinimumFeeFailed", exceptionToJson( e, clientId ) );
				Log.e( TAG, "GetMinimumFee failed", e );
			}
		} ).start();
	}


	public void buildTransaction( final String accountId, final String toAddress, final String kinAmount, final int fee, final String memo )
	{
		new Thread( () -> {
			try
			{
				Log.i( TAG, "Preparing to build transaction. toAddress: " + toAddress + ", memo: " + memo );
				Transaction transaction;
				if( TextUtils.isEmpty( memo ) )
					transaction = _accounts.get( accountId ).buildTransactionSync( toAddress, new BigDecimal( kinAmount ), fee );
				else
					transaction = _accounts.get( accountId ).buildTransactionSync( toAddress, new BigDecimal( kinAmount ), fee, memo );

				_transactions.put( transaction.getId().id(), transaction );
				unitySendMessage("BuildTransactionSucceeded", transactionToJson( transaction, accountId ) );
			}
			catch( Exception e )
			{
				unitySendMessage( "BuildTransactionFailed", exceptionToJson( e, accountId ) );
				Log.e( TAG, "BuildTransaction failed", e );
			}
		} ).start();
	}


	public void sendTransaction( final String accountId, final String id )
	{
		new Thread( () -> {
			try
			{
				Transaction transaction = _transactions.remove( id );
				Log.i( TAG, "Preparing to send transaction: " + transaction.getDestination().getAccountId() );

				TransactionId transactionId = _accounts.get( accountId ).sendTransactionSync( transaction );
				unitySendMessage("SendTransactionSucceeded", callbackToJson( transactionId.id(), accountId ) );
			}
			catch( Exception e )
			{
				unitySendMessage( "SendTransactionFailed", exceptionToJson( e, accountId ) );
				Log.e( TAG, "SendTransaction failed", e );
			}
		} ).start();
	}


	public void sendWhitelistTransaction( final String accountId, final String id, final String whitelist )
	{
		new Thread( () -> {
			try
			{
				Transaction transaction = _transactions.remove( id );
				Log.i( TAG, "Preparing to send transaction: " + transaction.getDestination().getAccountId() );

				TransactionId transactionId = _accounts.get( accountId ).sendWhitelistTransactionSync( whitelist );
				unitySendMessage("SendTransactionSucceeded", callbackToJson( transactionId.id(), accountId ) );
			}
			catch( Exception e )
			{
				unitySendMessage( "SendTransactionFailed", exceptionToJson( e, accountId ) );
				Log.e( TAG, "SendTransaction failed", e );
			}
		} ).start();
	}


	public void addPaymentListener( final String accountId )
	{
		// we only need one listener on the native side. Multiple listeners can be added on the Unity side.
		if( _paymentListeners.containsKey( accountId ) )
			return;

		ListenerRegistration registration = _accounts.get( accountId ).addPaymentListener( paymentInfo -> unitySendMessage( "OnPayment", paymentInfoToJson( paymentInfo, accountId ) ) );
		_paymentListeners.put( accountId, registration );
	}


	public void removePaymentListener( String accountId )
	{
		if( _paymentListeners.containsKey( accountId ) )
		{
			_paymentListeners.get( accountId ).remove();
			_paymentListeners.remove( accountId );
		}
	}


	public void addBalanceListener( final String accountId )
	{
		// we only need one listener on the native side. Multiple listeners can be added on the Unity side.
		if( _balanceListeners.containsKey( accountId ) )
			return;

		ListenerRegistration registration = _accounts.get( accountId ).addBalanceListener( balance -> unitySendMessage( "OnBalance", callbackToJson( balance.value().toString(), accountId ) ) );
		_balanceListeners.put( accountId, registration );
	}


	public void removeBalanceListener( String accountId )
	{
		if( _balanceListeners.containsKey( accountId ) )
		{
			_balanceListeners.get( accountId ).remove();
			_balanceListeners.remove( accountId );
		}
	}


	public void addAccountCreationListener( final String accountId )
	{
		// we only need one listener on the native side. Multiple listeners can be added on the Unity side.
		if( _accountListeners.containsKey( accountId ) )
			return;

		ListenerRegistration registration = _accounts.get( accountId ).addAccountCreationListener( data -> unitySendMessage( "OnAccountCreated", accountId ) );
		_accountListeners.put( accountId, registration );
	}


	public void removeAccountCreationListener( String accountId )
	{
		if( _accountListeners.containsKey( accountId ) )
		{
			_accountListeners.get( accountId ).remove();
			_accountListeners.remove( accountId );
		}
	}

	//endregion

}
