using System.Collections;
using UnityEngine;


namespace Kin
{
	/// <summary>
	/// displays a simple immediate mode GUI that maps buttons directly to the plugin's public methods
	/// </summary>
	public class KinDemoUI : KinDemoUIBase, IPaymentListener, IBalanceListener, IAccountCreationListener
	{
		KinClient _client;
		KinAccount _account;
		Transaction _transaction;
		int _feeAmount = 100;

		string _sendToAddress = "GCV7RE24EL2LO2QPONLL4NGTPJUK326ZAWP4NZHCQ5CKE73IWSMM7QXG";
		string _importExportPassphrase = System.Guid.NewGuid().ToString();
		string _exportedAccountJson = "";
		bool _isAccountCreated;


		protected override void OnLeftColumnGUI()
		{
			GUILayout.Label( "Connecting to a service provider" );
			if( GUILayout.Button( "Create Kin Client" ) )
			{
				_client = new KinClient( Environment.Test, "test" );
			}


			if( _client != null && GUILayout.Button( "Free Kin Client" ) )
			{
				_client = null;
				_account = null;
			}


			if( _client == null )
				return;

			GUILayout.Space( 40 );
			GUILayout.Label( "Kin Client Operations" );
			if( GUILayout.Button( "Clear All Accounts" ) )
			{
				_client.ClearAllAccounts();
				removeListeners();
			}


			if( GUILayout.Button( "Get Account Count" ) )
			{
				Debug.Log( "account count: " + _client.GetAccountCount() );
			}


			if( GUILayout.Button( "Get Minimum Fee" ) )
			{
				ShowProgressWindow( "GetMinimumFee" );
				_client.GetMinimumFee( ( ex, fee ) =>
				{
					HideProgressWindow();
					if( ex == null )
					{
						Debug.Log( "Fee: " + fee );
						_feeAmount = fee;
					}
					else
					{
						Debug.LogError( "Get Minimum Fee Failed. " + ex );
					}
				});
			}


			GUILayout.Space( 40 );
			GUILayout.Label( "Creating and retrieving a Kin account" );
			if( GUILayout.Button( "Get or Create Kin Account" ) )
			{
				_isAccountCreated = false;
				_transaction = null;

				if( _client.HasAccount() )
				{
					_account = _client.GetAccount();
				}
				else
				{
					try
					{
						_account = _client.AddAccount();
					}
					catch( KinException e )
					{
						Debug.LogError( "error adding account: " + e );
						return;
					}
				}

				// if we found an account add blockchain listeners that will log the events as they happen
				if( _account != null )
					addListeners();
			}


			GUILayout.Space( 40 );
			GUILayout.Label( "Exported Account JSON:" );
			_exportedAccountJson = GUILayout.TextField( _exportedAccountJson );

			if( !string.IsNullOrEmpty( _exportedAccountJson ) && GUILayout.Button( "Import Account" ) )
			{
				try
				{
					_account = _client.ImportAccount( _exportedAccountJson, _importExportPassphrase );
				}
				catch( KinException e )
				{
					Debug.LogError( e );
				}

				// if we imported an account add blockchain listeners that will log the events as they happen
				if( _account != null )
					addListeners();
			}


			if( _account == null )
				return;


			if( GUILayout.Button( "Delete Account" ) )
			{
				try
				{
					_client.DeleteAccount();
					removeListeners();
				}
				catch( KinException e )
				{
					Debug.LogError( "error deleting account: " + e );
					return;
				}
			}
		}


		protected override void OnRightColumnGUI()
		{
			if( BottomButton( "Show Logs" ) )
			{
				ToggleEditMode();
			}

			if( _account == null )
				return;

			GUILayout.Label( "Onboarding" );
			if( !_isAccountCreated && GUILayout.Button( "Create Account" ) )
			{
				ShowProgressWindow( "Create Account Onboarding" );
				StartCoroutine( KinOnboarding.CreateAccount( _account.GetPublicAddress(), didSucceed =>
				{
					_isAccountCreated = didSucceed;
					HideProgressWindow();
				} ) );
			}
			else if( _isAccountCreated )
			{
				GUILayout.Label( "Account Already Onboarded" );
			}


			GUILayout.Space( 40 );
			GUILayout.Label( "Account Information" );
			if( GUILayout.Button( "Get Public Address" ) )
			{
				Debug.Log( "Public address: " + _account.GetPublicAddress() );
			}


			if( GUILayout.Button( "Query Account Status" ) )
			{
				ShowProgressWindow( "GetStatus" );
				_account.GetStatus( ( ex, status ) =>
				{
					HideProgressWindow();
					if( ex == null )
						Debug.Log( "Account status: " + status );
					else
						Debug.LogError( "Get Account Status Failed. " + ex );
				});
			}


			if( GUILayout.Button( "Export Account" ) )
			{
				try
				{
					_exportedAccountJson = _account.Export( _importExportPassphrase );
					Debug.Log( "exported account with passphrase: " + _importExportPassphrase );
					Debug.Log( "exported account json:\n" + _exportedAccountJson );
				}
				catch( KinException ex )
				{
					Debug.LogError( ex );
				}
			}


			GUILayout.Space( 40 );
			GUILayout.Label( "Retrieving Balance" );
			if( GUILayout.Button( "Get Balance" ) )
			{
				ShowProgressWindow( "GetBalance" );
				_account.GetBalance( ( ex, balance ) =>
				{
					HideProgressWindow();
					if( ex == null )
						Debug.Log( "Balance: " + balance );
					else
						Debug.LogError( "Get Balance Failed. " + ex );
				});
			}


			GUILayout.Space( 40 );
			GUILayout.Label( "Transactions" );

			GUILayout.Label( "Send to:" );
			_sendToAddress = GUILayout.TextField( _sendToAddress );

			if( !string.IsNullOrEmpty( _sendToAddress ) && _isAccountCreated
				&& _transaction == null && GUILayout.Button( "Build Transaction" ) )
			{
				ShowProgressWindow( "BuildTransaction" );
				_account.BuildTransaction( _sendToAddress, 100, _feeAmount, ( ex, transaction ) =>
				{
					HideProgressWindow();
					if( ex == null )
					{
						Debug.Log( "Build Transaction: " + transaction );
						_transaction = transaction;
					}
					else
					{
						Debug.LogError( "Build Transaction Failed. " + ex );
					}
				});
			}

			if( _transaction != null && GUILayout.Button( "Send Transaction" ) )
			{
				ShowProgressWindow( "SendTransaction" );
				_account.SendTransaction( _transaction, ( ex, transactionId ) =>
				{
					HideProgressWindow();
					_transaction = null;
					if( ex == null )
						Debug.Log( "Send Transaction: " + transactionId );
					else
						Debug.LogError( "Send Transaction Failed. " + ex );
				});
			}
		}


		#region Blockchain Listeners

		void addListeners()
		{
			_account.AddPaymentListener( this );
			_account.AddBalanceListener( this );
			_account.AddAccountCreationListener( this );
		}


		void removeListeners()
		{
			if( _account == null )
				return;

			_account.RemovePaymentListener( this );
			_account.RemoveBalanceListener( this );
			_account.RemoveAccountCreationListener( this );

			_account = null;
			_transaction = null;
		}


		/// <summary>
		/// IPaymentListener implementation
		/// </summary>
		/// <param name="payment"></param>
		public void OnEvent( PaymentInfo payment )
		{
			Debug.Log( "On Payment: " + payment );
		}


		/// <summary>
		/// IBalanceListener implementation
		/// </summary>
		/// <param name="balance"></param>
		public void OnEvent( decimal balance )
		{
			Debug.Log( "On Balance: " + balance );
		}


		/// <summary>
		/// IAccountCreationListener implementation
		/// </summary>
		public void OnEvent()
		{
			Debug.Log( "On Account Created" );
			_isAccountCreated = true;
		}

		#endregion

	}
}
