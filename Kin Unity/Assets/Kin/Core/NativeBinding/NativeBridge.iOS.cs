using System;
using System.Runtime.InteropServices;


#if UNITY_IOS
namespace Kin
{
	class NativeBridgeIos : INativeBridge
	{
		public static readonly NativeBridgeIos Instance = new NativeBridgeIos();


#region KinClient

		[DllImport("__Internal")]
		static extern void _kinCreateClient( string clientId, int environment, string apiKey, string storeKey );

		public void CreateClient( string clientId, Environment environment, string apiKey, string storeKey = null )
		{
			_kinCreateClient( clientId, (int)environment, apiKey, storeKey );
		}


		[DllImport("__Internal")]
		static extern void _kinFreeCachedClient( string clientId );

		public void FreeCachedClient( string clientId )
		{
			_kinFreeCachedClient( clientId );
		}


		[DllImport("__Internal")]
		static extern string _kinImportAccount( string clientId, string accountId, string exportedJson, string passphrase );

		public string ImportAccount( string clientId, string accountId, string exportedJson, string passphrase )
		{
			return _kinImportAccount( clientId, accountId, exportedJson, passphrase );
		}


		[DllImport("__Internal")]
		static extern int _kinGetAccountCount( string clientId );

		public int GetAccountCount( string clientId )
		{
			return _kinGetAccountCount( clientId );;
		}


		[DllImport("__Internal")]
		static extern string _kinAddAccount( string clientId, string accountId );

		public string AddAccount( string clientId, string accountId )
		{
			return _kinAddAccount( clientId, accountId );
		}


		[DllImport("__Internal")]
		static extern bool _kinGetAccount( string clientId, string accountId, int index );

		public bool GetAccount( string clientId, string accountId, int index )
		{
			return _kinGetAccount( clientId, accountId, index );
		}


		[DllImport("__Internal")]
		static extern string _kinDeleteAccount( string clientId, int index );

		public string DeleteAccount( string clientId, int index )
		{
			return _kinDeleteAccount( clientId, index );
		}


		[DllImport("__Internal")]
		static extern void _kinClearAllAccounts( string clientId );

		public void ClearAllAccounts( string clientId )
		{
			_kinClearAllAccounts( clientId );
		}


		[DllImport("__Internal")]
		static extern void _kinGetMinimumFee( string clientId );

		public void GetMinimumFee( string clientId )
		{
			_kinGetMinimumFee( clientId );
		}

        [DllImport("__Internal")]
        static extern void _kinRestoreAccount(string clientId);

        public void RestoreAccount(string clientId)
        {
            _kinRestoreAccount(clientId); 
        }

#endregion


#region KinAccount

		[DllImport("__Internal")]
		static extern void _kinFreeCachedAccount( string accountId );

		public void FreeCachedAccount( string accountId )
		{
			_kinFreeCachedAccount( accountId );
		}


		[DllImport("__Internal")]
		static extern string _kinGetPublicAddress( string accountId );

		public string GetPublicAddress( string accountId )
		{
			return _kinGetPublicAddress( accountId );
		}


		[DllImport("__Internal")]
		static extern string _kinExport( string accountId, string passphrase );

		public string Export( string accountId, string passphrase )
		{
			return _kinExport( accountId, passphrase );
		}


		[DllImport("__Internal")]
		static extern void _kinGetStatus( string accountId );

		public void GetStatus( string accountId )
		{
			_kinGetStatus( accountId );
		}


		[DllImport("__Internal")]
		static extern void _kinGetBalance( string accountId );

		public void GetBalance( string accountId )
		{
			_kinGetBalance( accountId );
		}


		[DllImport("__Internal")]
		static extern void _kinBuildTransaction( string accountId, string toAddress, string kinAmount, int fee, string memo );

		public void BuildTransaction( string accountId, string toAddress, string kinAmount, int fee, string memo = null )
		{
			_kinBuildTransaction( accountId, toAddress, kinAmount, fee, memo );
		}


		[DllImport("__Internal")]
		static extern void _kinSendTransaction( string accountId, string transactionId );

		//public void SendTransaction( string accountId, string toAddress, decimal kinAmount, string memo = null )
		public void SendTransaction( string accountId, string transactionId )
		{
			_kinSendTransaction( accountId, transactionId );
		}


		[DllImport("__Internal")]
		static extern void _kinSendWhitelistTransaction( string accountId, string transactionId, string whitelist );

		public void SendWhitelistTransaction( string accountId, string transactionId, string whitelist )
		{
			_kinSendWhitelistTransaction( accountId, transactionId, whitelist );
		}


		[DllImport("__Internal")]
		static extern void _kinAddPaymentListener( string accountId );

		public void AddPaymentListener( string accountId )
		{
			_kinAddPaymentListener( accountId );
		}


		[DllImport("__Internal")]
		static extern void _kinRemovePaymentListener( string accountId );

		public void RemovePaymentListener( string accountId )
		{
			_kinRemovePaymentListener( accountId );
		}


		[DllImport("__Internal")]
		static extern void _kinAddBalanceListener( string accountId );

		public void AddBalanceListener( string accountId )
		{
			_kinAddBalanceListener( accountId );
		}


		[DllImport("__Internal")]
		static extern void _kinRemoveBalanceListener( string accountId );

		public void RemoveBalanceListener( string accountId )
		{
			_kinRemoveBalanceListener( accountId );
		}


		[DllImport("__Internal")]
		static extern void _kinAddAccountCreationListener( string accountId );

		public void AddAccountCreationListener( string accountId )
		{
			_kinAddAccountCreationListener( accountId );
		}


		[DllImport("__Internal")]
		static extern void _kinRemoveAccountCreationListener( string accountId );

		public void RemoveAccountCreationListener( string accountId )
		{
			_kinRemoveAccountCreationListener( accountId );
		}

        [DllImport("__Internal")]
        static extern void _kinBackupAccount(string accountId);

        public void BackupAccount(string accountId, string clientId)
        {
            // clientId is not used on iOS, but the method signature needs 
            // it because of the unified interface with android
            _kinBackupAccount( accountId );
        }

#endregion

    }
}
#endif