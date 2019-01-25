using System.Collections;
using System.Collections.Generic;
using Kin;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;


public class AddAndCreateAccountTest : KinMonoBehaviourTestBase
{
	IEnumerator Start()
	{
		AddAccount();
		
		yield return StartCoroutine( CreateAccount() );

        yield return StartCoroutine( CheckAccountStatus( AccountStatus.Created ) );

		_isTestFinished = true;
	}
}
