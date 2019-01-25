using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Kin
{
	/// <summary>
	/// represents a native KinClient. This class will call the correct native code based on the currently running platform.
	/// </summary>
	public class KinClient
	{
		readonly string _clientId;


		/// <summary>
		/// creates a new KinClient object
		/// </summary>
		/// <param name="environment"></param>
		/// <param name="appId"></param>
		/// <param name="storeKey"></param>
		public KinClient( Environment environment, string appId, string storeKey = null )
		{
			Assert.IsTrue(appId.Length == 4);
			_clientId = Utils.RandomString();
			NativeBridge.Get().CreateClient( _clientId, environment, appId, storeKey );
		}


		~KinClient()
		{
			// we have to delay this call and do it on the main thread to avoid JNI issues
			KinManager.Manager.queueDelayedCall( () =>
			{
				NativeBridge.Get().FreeCachedClient( _clientId );
			} );
		}


		/// <summary>
		/// imports an account that was exported via the KinAccount.Export method
		/// </summary>
		/// <param name="exportedJson"></param>
		/// <param name="passphrase"></param>
		/// <returns></returns>
		public KinAccount ImportAccount( string exportedJson, string passphrase )
		{
			var accountId = Utils.RandomString();
			var error = NativeBridge.Get().ImportAccount( _clientId, accountId, exportedJson, passphrase );

			if( !string.IsNullOrEmpty( error ) )
				throw KinException.FromNativeErrorJson( error );
			return new KinAccount( accountId );
		}


		/// <summary>
		/// gets the total number of KinAccounts on this device for this KinClient
		/// </summary>
		/// <returns></returns>
		public int GetAccountCount()
		{
			return NativeBridge.Get().GetAccountCount( _clientId );
		}


		/// <summary>
		/// returns if an account is available on this device
		/// </summary>
		/// <returns></returns>
		public bool HasAccount()
		{
			return GetAccountCount() > 0;
		}


		/// <summary>
		/// adds a new account to the KinClient. Throws an exception if any issues arise while adding it.
		/// </summary>
		/// <returns></returns>
		public KinAccount AddAccount()
		{
			var accountId = Utils.RandomString();
			var error = NativeBridge.Get().AddAccount( _clientId, accountId );

			if( !string.IsNullOrEmpty( error ) )
				throw KinException.FromNativeErrorJson( error );
			return new KinAccount( accountId );
		}


		/// <summary>
		/// gets the KinAccount at index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public KinAccount GetAccount( int index = 0 )
		{
			var accountId = Utils.RandomString();
			if( NativeBridge.Get().GetAccount( _clientId, accountId, index ) )
				return new KinAccount( accountId );
			return null;
		}


		/// <summary>
		/// attempts to delete the account at index. Throws an exception if it fails.
		/// </summary>
		/// <param name="index"></param>
		public void DeleteAccount( int index = 0 )
		{
			var error = NativeBridge.Get().DeleteAccount( _clientId, index );

			if( !string.IsNullOrEmpty( error ) )
				throw KinException.FromNativeErrorJson( error );
		}


		/// <summary>
		/// removes all of the accounts on the device
		/// </summary>
		public void ClearAllAccounts()
		{
			NativeBridge.Get().ClearAllAccounts( _clientId );
		}


		/// <summary>
		/// gets the current minimum fee on the blockchain
		/// </summary>
		/// <param name="onComplete"></param>
		public void GetMinimumFee( Action<KinException, int> onComplete )
		{
			if( KinManager.onGetMinimumFee.ContainsKey( _clientId ) )
				throw new KinException( "KinClient request already in flight for this method. Wait for it to complete before requesting it again." );
			KinManager.onGetMinimumFee[_clientId] = onComplete;
			NativeBridge.Get().GetMinimumFee( _clientId );
		}

	}
}