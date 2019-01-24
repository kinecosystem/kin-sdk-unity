using System.Collections;
using System.Collections.Generic;
using Kin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;


public class BlockchainListenerTest : KinMonoBehaviourTestBase
{
	IEnumerator Start()
	{
		ImportActivatedAccount();

		AddBlockchainListeners();

		yield return StartCoroutine( WaitForAccountCreationListener() );

		yield return StartCoroutine( BuildTransaction( 100 ) );

		yield return StartCoroutine( SendTransaction() );

		yield return StartCoroutine( WaitForPaymentListener() );

		yield return StartCoroutine( WaitForBalanceChangedListener() );

		_isTestFinished = true;
	}
}
