using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Kin
{
	public static class KinOnboarding
	{
		class WhitelistPostData
		{
			public string envelope;
			public string network_id;

			public WhitelistPostData( Transaction transaction )
			{
				envelope = transaction.WhitelistableTransactionPayLoad;
				network_id = transaction.WhitelistableTransactionNetworkPassphrase;
			}
		}
		/// <summary>
		/// creates an account on the server. In production this should be replaced by calls to your own secure server!
		/// </summary>
		/// <param name="publicAddress"></param>
		/// <returns></returns>
		public static IEnumerator CreateAccount( string publicAddress, Action<bool> onComplete = null )
		{
			var url = "https://friendbot.developers.kinecosystem.com/?addr=" + publicAddress + "&amount=1000";
			var req = UnityWebRequest.Get( url );

			yield return req.SendWebRequest();

			if( req.isNetworkError || req.isHttpError )
			{
				Debug.Log( req.error );
				if( onComplete != null )
					onComplete( false );
			}
			else
			{
				Debug.Log( "response code: " + req.responseCode );
				Debug.Log( req.downloadHandler.text );
				if( onComplete != null )
					onComplete( true );
			}
		}


		/// <summary>
		/// whitelists a transaction
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		public static IEnumerator WhitelistTransaction( Transaction transaction, Action<string> onComplete = null )
		{
			var postDataObj = new WhitelistPostData( transaction );
			var postData = JsonUtility.ToJson( postDataObj );
			var rawPostData = Encoding.UTF8.GetBytes( postData );

			// UnityWebRequest does not work correclty when posting a JSON string so we use a byte[] and a hacky workaround
			var url = "https://whitelister-playground.developers.kinecosystem.com/whitelist";
			var req = UnityWebRequest.Post( url, "POST" );
			req.SetRequestHeader( "Content-Type", "application/json" );
			req.uploadHandler = new UploadHandlerRaw( rawPostData );

			yield return req.SendWebRequest();

			if( req.isNetworkError || req.isHttpError )
			{
				Debug.Log( req.error );
				if( onComplete != null )
					onComplete( null );
			}
			else
			{
				Debug.Log( "response code: " + req.responseCode );
				Debug.Log( req.downloadHandler.text );
				if( onComplete != null )
					onComplete( req.downloadHandler.text );
			}
		}

	}
}