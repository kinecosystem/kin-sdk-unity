using System;
using System.Collections.Generic;
using UnityEngine;


namespace Kin
{
	/// <summary>
	/// KinManager lives on a persistant GameObject so that callbacks from native code can always be sent to it. Nothing in here
	/// needs to be accessed from public code.
	/// </summary>
	public class KinManager : MonoBehaviour
	{
		/// <summary>
		/// helper struct for deserializing JSON into two strings for callbacks from native code
		/// </summary>
		struct CallbackParam
		{
			public string AccountId;
			public string Value;
		}

		internal static Dictionary<string, List<IPaymentListener>> paymentListeners = new Dictionary<string, List<IPaymentListener>>();
		internal static Dictionary<string, List<IBalanceListener>> balanceListeners = new Dictionary<string, List<IBalanceListener>>();
		internal static Dictionary<string, List<IAccountCreationListener>> accountCreationListeners = new Dictionary<string, List<IAccountCreationListener>>();

		internal static Dictionary<string, Action<KinException, int>> onGetMinimumFee = new Dictionary<string, Action<KinException, int>>();
		internal static Dictionary<string, Action<KinException, AccountStatus>> onGetStatus = new Dictionary<string, Action<KinException, AccountStatus>>();
		internal static Dictionary<string, Action<KinException, decimal>> onGetBalance = new Dictionary<string, Action<KinException, decimal>>();
		internal static Dictionary<string, Action<KinException, string>> onSendTransaction = new Dictionary<string, Action<KinException, string>>();
		internal static Dictionary<string, Action<KinException, Transaction>> onBuildTransaction = new Dictionary<string, Action<KinException, Transaction>>();

		internal static KinManager Manager;
		Queue<Action> _actionQueue = new Queue<Action>();
		

		/// <summary>
		/// creates on the fly a GameObject with this script on it named in such a way that native code can access it
		/// </summary>
		static KinManager()
		{
			// try/catch this just in case a user sticks this class on a GameObject in the scene
			try
			{
				// first we see if we already exist in the scene
				var obj = FindObjectOfType<KinManager>();
				if( obj != null )
					return;

				// create a new GO for our manager. This name is crucial as all native code communicates with this class by its name.
				var managerGO = new GameObject( "KinManager" );
				Manager = managerGO.AddComponent<KinManager>();

				DontDestroyOnLoad( managerGO );
			}
			catch( UnityException )
			{
				Debug.LogWarning( "It looks like you have the KinManager on a GameObject in your scene. It will be added to your scene at runtime automatically for you. Please remove the script from your scene." );
			}
		}

		
		/// <summary>
		/// adds an Action to a queue that will be called on the main thread
		/// </summary>
		/// <param name="action"></param>
		public void queueDelayedCall( Action action )
		{
			lock( this )
			{
				_actionQueue.Enqueue( action );
			}
		}


		/// <summary>
		/// we use Update only to queue finalizer calls and call them on the main thread
		/// </summary>
		void Update()
		{
			if( _actionQueue.Count > 0 )
			{
				lock( this )
				{
					while( _actionQueue.Count > 0 )
						_actionQueue.Dequeue()();
				}
			}
		}


		/// <summary>
		/// this ensures we always have a KinManager available in the scene so that native code can call back to Unity
		/// </summary>
		[RuntimeInitializeOnLoadMethod]
		static internal void noop() { }


		#region KinClient callbacks

		void GetMinimumFeeSucceeded( string json )
		{
			var param = JsonUtility.FromJson<CallbackParam>( json );
			onGetMinimumFee.FireActionInDict( param.AccountId, null, int.Parse( param.Value ) );
		}


		void GetMinimumFeeFailed( string error )
		{
			var ex = KinException.FromNativeErrorJson( error );
			onGetMinimumFee.FireActionInDict( ex.AccountId, ex, -1 );
		}

		#endregion


		#region KinAccount callbacks

		void GetStatusSucceeded( string json )
		{
			var param = JsonUtility.FromJson<CallbackParam>( json );
			var status = (AccountStatus)int.Parse( param.Value );
			onGetStatus.FireActionInDict( param.AccountId, null, status );
		}


		void GetStatusFailed( string error )
		{
			var ex = KinException.FromNativeErrorJson( error );
			onGetStatus.FireActionInDict( ex.AccountId, ex, default( AccountStatus ) );
		}


		void GetBalanceSucceeded( string json )
		{
			var param = JsonUtility.FromJson<CallbackParam>( json );
			var balance = decimal.Parse( param.Value, System.Globalization.NumberStyles.Float );
			onGetBalance.FireActionInDict( param.AccountId, null, balance );
		}


		void GetBalanceFailed( string json )
		{
			var ex = KinException.FromNativeErrorJson( json );
			onGetBalance.FireActionInDict( ex.AccountId, ex, default( decimal ) );
		}


		void BuildTransactionSucceeded( string json )
		{
			var transaction = JsonUtility.FromJson<Transaction>( json );
			onBuildTransaction.FireActionInDict( transaction.AccountId, null, transaction );
		}


		void BuildTransactionFailed( string json )
		{
			var ex = KinException.FromNativeErrorJson( json );
			onBuildTransaction.FireActionInDict( ex.AccountId, ex, null );
		}


		void SendTransactionSucceeded( string json )
		{
			var param = JsonUtility.FromJson<CallbackParam>( json );
			onSendTransaction.FireActionInDict( param.AccountId, null, param.Value );
		}


		void SendTransactionFailed( string json )
		{
			var ex = KinException.FromNativeErrorJson( json );
			onSendTransaction.FireActionInDict( ex.AccountId, ex, null );
		}

		#endregion


		#region Listener callbacks

		void OnPayment( string json )
		{
			var paymentInfo = JsonUtility.FromJson<PaymentInfo>( json );
			if( paymentListeners.ContainsKey( paymentInfo.AccountId ) )
			{
				foreach( var listener in paymentListeners[paymentInfo.AccountId] )
					listener.OnEvent( paymentInfo );
			}
		}


		void OnBalance( string json )
		{
			var param = JsonUtility.FromJson<CallbackParam>( json );
			var balance = decimal.Parse( param.Value, System.Globalization.NumberStyles.Float );
			if( balanceListeners.ContainsKey( param.AccountId ) )
			{
				foreach( var listener in balanceListeners[param.AccountId] )
					listener.OnEvent( balance );
			}
		}


		void OnAccountCreated( string accountId )
		{
			if( accountCreationListeners.ContainsKey( accountId ) )
			{
				foreach( var listener in accountCreationListeners[accountId] )
					listener.OnEvent();
			}
		}

		#endregion

	}
}