using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;


public class TransactionVerificationTest : KinMonoBehaviourTestBase
{
	StreamWriter GetTransactionLogFile( bool deleteContents = false )
	{
		var path = Path.Combine( Application.persistentDataPath, "transactionLog.txt" );
		if( deleteContents && File.Exists( path ) )
			File.Delete( path );
		
		var file = new StreamWriter( path, true );
		if( deleteContents )
			file.WriteLine( "Source Address, Dest Address, Sent Amount, Sent Memo, Transaction Id, Source Address, Dest Address, Amount, Memo, Hash" );
		return file;
	}


	IEnumerator Start()
	{
		var file = GetTransactionLogFile( true );

		Debug.Log( "----- starting TransactionVerificationTest -----" );

		AddAccount();
		
		yield return StartCoroutine( CreateAccount() );

		AddBlockchainListeners();

		for( var i = 0; i < 250; i++ )
		{
			var memo = Path.GetRandomFileName();
			var amount = Random.Range( 5, 20 );

			yield return StartCoroutine( BuildTransactionWithMemo( amount, memo ) );

			yield return StartCoroutine( SendTransaction() );

			yield return StartCoroutine( WaitForPaymentListener() );

			Debug.LogFormat( "---- transIds match: {0}, memos match: {1}", _transaction.Id == _paymentInfo.Hash, memo == _paymentInfo.Memo );

			file.WriteLine( string.Format( "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}",
				_account.GetPublicAddress(), _sendToAddress, amount, memo, _transaction.Id,
				_paymentInfo.SourcePublicKey, _paymentInfo.DestinationPublicKey, _paymentInfo.Amount, _paymentInfo.Memo, _paymentInfo.Hash ) );
			
			// reset state
			_transaction = null;
			ResetBlockchainListeners();
		}
		file.Close();

		_isTestFinished = true;
	}
}
