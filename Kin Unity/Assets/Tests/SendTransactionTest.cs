using System.Collections;
using System.Collections.Generic;
using Kin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;


public class SendTransactionTest : KinMonoBehaviourTestBase
{
	IEnumerator Start()
	{
		ImportActivatedAccount();

		yield return StartCoroutine( CheckAccountBalance( 100 ) );

		yield return StartCoroutine(BuildTransactionFail());

		yield return StartCoroutine( BuildTransaction( 100 ) );

		yield return StartCoroutine( SendTransaction() );

		_isTestFinished = true;
	}
}
