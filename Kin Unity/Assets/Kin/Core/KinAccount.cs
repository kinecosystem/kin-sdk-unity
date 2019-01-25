using System;
using System.Collections;
using UnityEngine;


namespace Kin
{
	/// <summary>
	/// the main interface to access a KinAccount
	/// </summary>
	public class KinAccount
	{
		readonly string _accountId;


		internal KinAccount( string accountId )
		{
			_accountId = accountId;
		}


		~KinAccount()
		{
			// we have to delay this call and do it on the main thread to avoid JNI issues
			KinManager.Manager.queueDelayedCall( () => {
				NativeBridge.Get().FreeCachedAccount( _accountId );

				// cleanup any stray listeners in case someone forgot to remove them manually
				if( KinManager.paymentListeners.ContainsKey( _accountId ) )
				{
					KinManager.paymentListeners.Remove( _accountId );
					NativeBridge.Get().RemovePaymentListener( _accountId );
				}

				if( KinManager.balanceListeners.ContainsKey( _accountId ) )
				{
					KinManager.balanceListeners.Remove( _accountId );
					NativeBridge.Get().RemoveBalanceListener( _accountId );
				}

				if( KinManager.accountCreationListeners.ContainsKey( _accountId ) )
				{
					KinManager.accountCreationListeners.Remove( _accountId );
					NativeBridge.Get().RemoveAccountCreationListener( _accountId );
				}
			} );
		}


		/// <summary>
		/// helper method that ensures only one call for each async type is in flight at at time
		/// </summary>
		/// <param name="dict"></param>
		void throwIfRequestInFlight( IDictionary dict )
		{
			if( dict.Contains( _accountId ) )
				throw new KinException( "KinAccount request already in flight for this method. Wait for it to complete before requesting it again." );
		}


		/// <summary>
		/// gets the public address of the KinAccount which can be used to send transactions from another account
		/// </summary>
		/// <returns></returns>
		public string GetPublicAddress()
		{
			return NativeBridge.Get().GetPublicAddress( _accountId );
		}


		/// <summary>
		/// exports an account. The returned JSON can be imported on a different device via KinClient.ImportAccount.
		/// </summary>
		/// <param name="passphrase"></param>
		/// <returns></returns>
		public string Export( string passphrase )
		{
			var json = NativeBridge.Get().Export( _accountId, passphrase );

			var exception = KinException.FromNativeErrorJson( json );
			if( exception != null )
			{
				UnityEngine.Debug.Log( exception );
				throw exception;
			}

			return json;
		}


		/// <summary>
		/// gets the AccountStatus of this KinAccount
		/// </summary>
		/// <param name="onComplete"></param>
		public void GetStatus( Action<KinException, AccountStatus> onComplete )
		{
			throwIfRequestInFlight( KinManager.onGetStatus );
			KinManager.onGetStatus[_accountId] = onComplete;
			NativeBridge.Get().GetStatus( _accountId );
		}


		/// <summary>
		/// gets the balance available in this KinAccount
		/// </summary>
		/// <param name="onComplete"></param>
		public void GetBalance( Action<KinException, decimal> onComplete )
		{
			throwIfRequestInFlight( KinManager.onGetBalance );
			KinManager.onGetBalance[_accountId] = onComplete;
			NativeBridge.Get().GetBalance( _accountId );
		}


		/// <summary>
		/// builds a Transaction object in preperation for sending or whitelisting the Transaction
		/// </summary>
		/// <param name="toAddress"></param>
		/// <param name="kinAmount"></param>
		/// <param name="fee"></param>
		/// <param name="onComplete"></param>
		public void BuildTransaction( string toAddress, decimal kinAmount, int fee, Action<KinException, Transaction> onComplete )
		{
			BuildTransaction( toAddress, kinAmount, fee, null, onComplete );
		}


		public void BuildTransaction( string toAddress, decimal kinAmount, int fee, string memo, Action<KinException, Transaction> onComplete )
		{
			throwIfRequestInFlight( KinManager.onBuildTransaction );
			KinManager.onBuildTransaction[_accountId] = onComplete;
			NativeBridge.Get().BuildTransaction( _accountId, toAddress, kinAmount.ToString(), fee, memo );
		}


		/// <summary>
		/// sends a transaction to the KinAccount with toAddress with a memo. Note that the memo must be a maximum of
		/// 21 bytes
		/// </summary>
		/// <param name="toAddress"></param>
		/// <param name="transaction"></param>
		/// <param name="onComplete"></param>
		public void SendTransaction( Transaction transaction, Action<KinException, string> onComplete )
		{
			throwIfRequestInFlight( KinManager.onSendTransaction );
			KinManager.onSendTransaction[_accountId] = onComplete;
			NativeBridge.Get().SendTransaction( _accountId, transaction.Id );
		}


		/// <summary>
		/// sends a whitelisted, zero-fee transaction
		/// </summary>
		/// <param name="transactionId"></param>
		/// <param name="whitelist"></param>
		/// <param name="onComplete"></param>
		public void SendWhitelistTransaction( string transactionId, string whitelist, Action<KinException, string> onComplete )
		{
			throwIfRequestInFlight( KinManager.onSendTransaction );
			KinManager.onSendTransaction[_accountId] = onComplete;
			NativeBridge.Get().SendWhitelistTransaction( _accountId, transactionId, whitelist );
		}


		/// <summary>
		/// adds a listener that will be called anytime this KinAccount receives a payment
		/// </summary>
		/// <param name="listener"></param>
		public void AddPaymentListener( IPaymentListener listener )
		{
			KinManager.paymentListeners.AddIfNotPresent( _accountId, listener );
			NativeBridge.Get().AddPaymentListener( _accountId );
		}


		/// <summary>
		/// removes the payment listener
		/// </summary>
		/// <param name="listener"></param>
		public void RemovePaymentListener( IPaymentListener listener )
		{
			KinManager.paymentListeners.RemoveIfPresent( _accountId, listener );
			NativeBridge.Get().RemovePaymentListener( _accountId );
		}


		/// <summary>
		/// adds a listener that will be called anytime this KinAccount balance changes
		/// </summary>
		/// <param name="listener"></param>
		public void AddBalanceListener( IBalanceListener listener )
		{
			KinManager.balanceListeners.AddIfNotPresent( _accountId, listener );
			NativeBridge.Get().AddBalanceListener( _accountId );
		}


		/// <summary>
		/// removes the balance listener
		/// </summary>
		/// <param name="listener"></param>
		public void RemoveBalanceListener( IBalanceListener listener )
		{
			KinManager.balanceListeners.RemoveIfPresent( _accountId, listener );
			NativeBridge.Get().RemoveBalanceListener( _accountId );
		}


		/// <summary>
		/// adds a listener that will be called when an account is created
		/// </summary>
		/// <param name="listener"></param>
		public void AddAccountCreationListener( IAccountCreationListener listener )
		{
			KinManager.accountCreationListeners.AddIfNotPresent( _accountId, listener );
			NativeBridge.Get().AddAccountCreationListener( _accountId );
		}


		/// <summary>
		/// removes the account creation listener
		/// </summary>
		/// <param name="listener"></param>
		public void RemoveAccountCreationListener( IAccountCreationListener listener )
		{
			KinManager.accountCreationListeners.RemoveIfPresent( _accountId, listener );
			NativeBridge.Get().RemoveAccountCreationListener( _accountId );
		}
	}
}