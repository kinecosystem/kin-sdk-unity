using System;


namespace Kin
{
	/// <summary>
	/// while this interface isnt technically necessary, it is used as a safety measure to ensure the iOS, Editor and Android
	/// implementations always include all the required methods. By doing it like that we get a compiler warning if a method
	/// is missing on one of the platforms.
	/// </summary>
	interface INativeBridge
	{
		#region KinClient

		void CreateClient( string clientId, Environment environment, string apiKey, string storeKey = null );

		void FreeCachedClient( string clientId );

		string ImportAccount( string clientId, string accountId, string exportedJson, string passphrase );

		int GetAccountCount( string clientId );

		string AddAccount( string clientId, string accountId );

		bool GetAccount( string clientId, string accountId, int index );

		string DeleteAccount( string clientId, int index );

		void ClearAllAccounts( string clientId );

		void GetMinimumFee( string clientId );

		#endregion


		#region KinAccount

		void FreeCachedAccount( string accountId );

		string GetPublicAddress( string accountId );

		string Export( string accountId, string passphrase );

		void Activate( string accountId );

		void GetStatus( string accountId );

		void GetBalance( string accountId );

		void BuildTransaction( string accountId, string toAddress, string kinAmount, int fee, string memo = null );

		void SendTransaction( string accountId, string transactionId );

		void SendWhitelistTransaction( string accountId, string transactionId, string whitelist );

		void AddPaymentListener( string accountId );

		void RemovePaymentListener( string accountId );

		void AddBalanceListener( string accountId );

		void RemoveBalanceListener( string accountId );

		void AddAccountCreationListener( string accountId );

		void RemoveAccountCreationListener( string accountId );

		#endregion
	}
}