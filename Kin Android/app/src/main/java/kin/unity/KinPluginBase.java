package kin.unity;


import android.app.Activity;
import android.util.Log;
import android.widget.Toast;

import org.json.JSONException;
import org.json.JSONObject;

import java.lang.reflect.Field;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;

import kin.sdk.PaymentInfo;
import kin.sdk.Transaction;


abstract class KinPluginBase
{
	static final String TAG = "KinUnity";

	private Class<?> _unityPlayerClass;
	private Field _unityPlayerActivityField;
	private Method _unitySendMessageMethod;

	// this can be set manually for the use case of testing outside of Unity
	public Activity _fallbackActivity;


	protected KinPluginBase()
	{
		// dynamically find the Unity bits we need to avoid having to link against their library
		try
		{
			_unityPlayerClass = Class.forName( "com.unity3d.player.UnityPlayer" );
			_unityPlayerActivityField = _unityPlayerClass.getField( "currentActivity" );
			_unitySendMessageMethod = _unityPlayerClass.getMethod( "UnitySendMessage", String.class, String.class, String.class );
		}
		catch( ClassNotFoundException e )
		{
			Log.i( TAG, "could not find UnityPlayer class: " + e.getMessage() );
		}
		catch( NoSuchFieldException e )
		{
			Log.i( TAG, "could not find currentActivity field: " + e.getMessage() );
		}
		catch( Exception e )
		{
			Log.i( TAG, "unknown exception occurred locating getActivity(): " + e.getMessage() );
		}
	}


	/**
	 * dynamically fetches the Activity
	 * @return currentActivity
	 */
	public Activity getActivity()
	{
		if( _unityPlayerActivityField != null )
		{
			try
			{
				Activity activity = (Activity)_unityPlayerActivityField.get( _unityPlayerClass );
				if( activity == null )
					Log.i( TAG, "The Unity Activity does not exist" );

				return activity;
			}
			catch( Exception e )
			{
				Log.i( TAG, "error getting currentActivity: " + e.getMessage() );
			}
		}

		return _fallbackActivity;
	}


	/**
	 * calls through to UnitySendMessage
	 * @param method
	 * @param parameter
	 */
	protected void unitySendMessage( final String method, String parameter )
	{
		if( parameter == null )
			parameter = "";

		// Try for the real UnitySendMessage first
		if( _unitySendMessageMethod != null )
		{
			try
			{
				_unitySendMessageMethod.invoke( null, "KinManager", method, parameter );
			}
			catch( IllegalArgumentException e )
			{
				Log.i( TAG, "could not find UnitySendMessage method: " + e.getMessage() );
			}
			catch( IllegalAccessException e )
			{
				Log.i( TAG, "could not find UnitySendMessage method: " + e.getMessage() );
			}
			catch( InvocationTargetException e )
			{
				Log.i( TAG, "could not find UnitySendMessage method: " + e.getMessage() );
			}
		}
		else
		{
			Log.i( TAG, "UnitySendMessage: KinManager, " + method + ", " + parameter );

			final String finalParameter = parameter;
			runSafelyOnUiThread( () -> Toast.makeText( getActivity(), String.format( "UnitySendMessage:\n%s\n%s", method, finalParameter ), Toast.LENGTH_LONG ).show() );
		}
	}


	/**
	 * runs the Runnable on the UI thread in a try/catch
	 */
	protected void runSafelyOnUiThread( final Runnable r )
	{
		getActivity().runOnUiThread( () -> {
			try
			{
				r.run();
			}
			catch( Exception e )
			{
				Log.e( TAG, "Exception running command on UI thread: " + e.getMessage() );
			}
		} );
	}


	/**
	 * converts a PaymentInfo object to JSON for transport back to Unity
	 * @param payment
	 * @return
	 */
	protected String paymentInfoToJson( PaymentInfo payment, String accountId )
	{
		JSONObject json = new JSONObject();

		try
		{
			json.put( "_Amount", payment.amount().toString() );
			json.put( "CreatedAt", payment.createdAt() );
			json.put( "DestinationPublicKey", payment.destinationPublicKey() );
			json.put( "SourcePublicKey", payment.sourcePublicKey() );
			json.put( "Hash", payment.hash().id() );
			json.put( "Memo", payment.memo() );

			json.put( "AccountId", accountId );
		}
		catch( JSONException e )
		{
			e.printStackTrace();
		}

		return json.toString();
	}


	/**
	 * Converts an Exception into JSON
	 * @param ex
	 * @param accountId
	 * @return
	 */
	protected String exceptionToJson( Exception ex, String accountId )
	{
		JSONObject json = new JSONObject();

		try
		{
			json.put( "Message", ex.getMessage() );
			json.put( "NativeType", ex.getClass().getSimpleName() );

			if( accountId != null )
				json.put( "AccountId", accountId );
		}
		catch( JSONException e )
		{
			e.printStackTrace();
		}

		return json.toString();
	}


	/**
	 * Converts a callback value with accountId to JSON so that Unity knows where to route the data.
	 * @param value
	 * @param accountId
	 * @return
	 */
	protected String callbackToJson( String value, String accountId )
	{
		JSONObject json = new JSONObject();

		try
		{
			json.put( "Value", value );
			json.put( "AccountId", accountId );
		}
		catch( JSONException e )
		{
			e.printStackTrace();
		}

		return json.toString();
	}


	/**
	 * Converts a Transaction with accountId to JSON
	 * @param transaction
	 * @param accountId
	 * @return
	 */
	protected String transactionToJson( Transaction transaction, String accountId )
	{
		JSONObject json = new JSONObject();

		try
		{
			json.put( "AccountId", accountId );
			json.put( "Id", transaction.getId().id() );
			json.put( "WhitelistableTransactionPayLoad", transaction.getWhitelistableTransaction().getTransactionPayload() );
			json.put( "WhitelistableTransactionNetworkPassphrase", transaction.getWhitelistableTransaction().getNetworkPassphrase() );
		}
		catch( JSONException e )
		{
			e.printStackTrace();
		}

		return json.toString();
	}

}
