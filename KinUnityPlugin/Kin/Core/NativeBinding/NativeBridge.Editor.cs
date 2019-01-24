using System;


namespace Kin
{
	/// <summary>
	/// empty implemention of the INativeBridge interface for use in the editor and on unsupported platforms.
	/// </summary>
	class NativeBridgeEditor : INativeBridge
	{
		/// <summary>
		/// no need to bother caching this in the editor so we just return a new one each time
		/// </summary>
		/// <value>The instance.</value>
		static internal INativeBridge Instance { get { return new NativeBridgeEditor(); } }


		#region KinClient

		public void CreateClient( string clientId, Environment environment, string apiKey, string storeKey = null )
		{}


		public void FreeCachedClient( string clientId )
		{}


		public string ImportAccount( string clientId, string accountId, string exportedJson, string passphrase )
		{
			return null;
		}


		public int GetAccountCount( string clientId )
		{
			return 0;
		}


		public string AddAccount( string clientId, string accountId )
		{
			return null;
		}


		public bool GetAccount( string clientId, string accountId, int index )
		{
			return false;
		}


		public string DeleteAccount( string clientId, int index )
		{
			return null;
		}


		public void ClearAllAccounts( string clientId )
		{ }


		public void GetMinimumFee( string clientId )
		{}

		#endregion


		#region KinAccount

		public void FreeCachedAccount( string accountId )
		{}


		public string GetPublicAddress( string accountId )
		{
			return null;
		}


		public string Export( string accountId, string passphrase )
		{
			return "{}";
		}


		public void Activate( string accountId )
		{}


		public void GetStatus( string accountId )
		{}


		public void GetBalance( string accountId )
		{}


		public void BuildTransaction( string accountId, string toAddress, string kinAmount, int fee, string memo = null )
		{}


		public void SendTransaction( string accountId, string transactionId )
		{}


		public void SendWhitelistTransaction( string accountId, string transactionId, string whitelist )
		{}


		public void AddPaymentListener( string accountId )
		{}


		public void RemovePaymentListener( string accountId )
		{}


		public void AddBalanceListener( string accountId )
		{}


		public void RemoveBalanceListener( string accountId )
		{}


		public void AddAccountCreationListener( string accountId )
		{}


		public void RemoveAccountCreationListener( string accountId )
		{}

		#endregion

	}
}
