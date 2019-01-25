using System;
using UnityEngine;


#if UNITY_ANDROID
namespace Kin
{
	/// <summary>
	/// handles all JNI calls from C# to Java
	/// </summary>
	class NativeBridgeAndroid : INativeBridge
	{
		public static readonly NativeBridgeAndroid Instance = new NativeBridgeAndroid();
		AndroidJavaObject _plugin;


		internal NativeBridgeAndroid()
		{
			// find the plugin instance and cache it
			using( var pluginClass = new AndroidJavaClass( "kin.unity.KinPlugin" ) )
				_plugin = pluginClass.CallStatic<AndroidJavaObject>( "instance" );
		}


		#region KinClient

		public void CreateClient( string clientId, Environment environment, string apiKey, string storeKey = null )
		{
			_plugin.Call( "createClient", clientId, (int)environment, apiKey, storeKey );
		}


		public void FreeCachedClient( string clientId )
		{
			_plugin.Call( "freeCachedClient", clientId );
		}


		public string ImportAccount( string clientId, string accountId, string exportedJson, string passphrase )
		{
			return _plugin.Call<string>( "importAccount", clientId, accountId, exportedJson, passphrase );
		}


		public int GetAccountCount( string clientId )
		{
			return _plugin.Call<int>( "getAccountCount", clientId );
		}


		public string AddAccount( string clientId, string accountId )
		{
			return _plugin.Call<string>( "addAccount", clientId, accountId );
		}


		public bool GetAccount( string clientId, string accountId, int index )
		{
			return _plugin.Call<bool>( "getAccount", clientId, accountId, index );
		}


		public string DeleteAccount( string clientId, int index )
		{
			return _plugin.Call<string>( "deleteAccount", clientId, index );
		}


		public void ClearAllAccounts( string clientId )
		{
			_plugin.Call( "clearAllAccounts", clientId );
		}


		public void GetMinimumFee( string clientId )
		{
			_plugin.Call( "getMinimumFee", clientId );
		}

		#endregion


		#region KinAccount

		public void FreeCachedAccount( string accountId )
		{
			_plugin.Call( "freeCachedAccount", accountId );
		}


		public string GetPublicAddress( string accountId )
		{
			return _plugin.Call<string>( "getPublicAddress", accountId );
		}


		public string Export( string accountId, string passphrase )
		{
			return _plugin.Call<string>( "export", accountId, passphrase );
		}


		public void Activate( string accountId )
		{
			_plugin.Call( "activate", accountId );
		}


		public void GetStatus( string accountId )
		{
			_plugin.Call( "getStatus", accountId );
		}


		public void GetBalance( string accountId )
		{
			_plugin.Call( "getBalance", accountId );
		}


		public void BuildTransaction( string accountId, string toAddress, string kinAmount, int fee, string memo = null )
		{
			_plugin.Call( "buildTransaction", accountId, toAddress, kinAmount, fee, memo );
		}


		public void SendTransaction( string accountId, string transactionId )
		{
			_plugin.Call( "sendTransaction", accountId, transactionId );
		}


		public void SendWhitelistTransaction( string accountId, string transactionId, string whitelist )
		{
			_plugin.Call( "sendWhitelistTransaction", accountId, transactionId, whitelist );
		}


		public void AddPaymentListener( string accountId )
		{
			_plugin.Call( "addPaymentListener", accountId );
		}


		public void RemovePaymentListener( string accountId )
		{
			_plugin.Call( "removePaymentListener", accountId );
		}


		public void AddBalanceListener( string accountId )
		{
			_plugin.Call( "addBalanceListener", accountId );
		}


		public void RemoveBalanceListener( string accountId )
		{
			_plugin.Call( "removeBalanceListener", accountId );
		}


		public void AddAccountCreationListener( string accountId )
		{
			_plugin.Call( "addAccountCreationListener", accountId );
		}


		public void RemoveAccountCreationListener( string accountId )
		{
			_plugin.Call( "removeAccountCreationListener", accountId );
		}

		#endregion

	}
}
#endif