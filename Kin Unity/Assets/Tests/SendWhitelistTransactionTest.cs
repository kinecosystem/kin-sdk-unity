using System.Collections;
using System.Collections.Generic;
using Kin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;


public class SendWhitelistTransactionTest : KinMonoBehaviourTestBase
{
	IEnumerator Start()
	{
		ImportActivatedAccount();

		yield return StartCoroutine( CheckAccountBalance( 100 ) );

		yield return StartCoroutine( BuildTransaction( 100 ) );

		yield return StartCoroutine( WhitelistTransaction() );

		yield return StartCoroutine( SendWhitelistTransaction() );

		_isTestFinished = true;
	}
}
