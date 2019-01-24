using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kin
{
	/// <summary>
	/// represents a native Transaction object
	/// </summary>
	[Serializable]
	public class Transaction
	{
		public string Id;
		public string AccountId;
		public string WhitelistableTransactionPayLoad;
		public string WhitelistableTransactionNetworkPassphrase;


		public override string ToString()
		{
			return JsonUtility.ToJson( this, true );
		}
	}
}