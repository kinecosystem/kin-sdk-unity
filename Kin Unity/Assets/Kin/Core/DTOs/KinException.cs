using System;
using System.Collections.Generic;
using UnityEngine;


namespace Kin
{
	/// <summary>
	/// wraps NSError/Error on iOS and the suite of Exceptions in the Java SDK
	/// </summary>
	public class KinException : Exception
	{
		/// <summary>
		/// helper that we use to deserialize the JSON into since we don't own Exception
		/// </summary>
		struct KinExceptionData
		{
			public string Message;
			public int ErrorCode;
			public string NativeType;
			public string AccountId;
		}

		public int ErrorCode { get; private set; }
		public string NativeType { get; private set; }
		
		internal string AccountId;


		public static KinException FromNativeErrorJson( string json )
		{
			// we have to parse this out into a struct because Unity's JSON deserializer...
			var data = JsonUtility.FromJson<KinExceptionData>( json );
			if( data.Message == null )
				return null;

			return new KinException( data );
		}


		public KinException( string message ) : base( message )
		{}


		KinException( KinExceptionData data ) : base( data.Message )
		{
			ErrorCode = data.ErrorCode;
			NativeType = data.NativeType;
			AccountId = data.AccountId;
		}


		public override string ToString()
		{
			return string.Format( "Message: {0}, NativeType: {1}, ErrorCode: {2}, AccountId: {3}",
			Message, NativeType, ErrorCode, AccountId );
		}
	}
}