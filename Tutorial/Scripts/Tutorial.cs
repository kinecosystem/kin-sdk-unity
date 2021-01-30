using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private KinWrapper kinWrapper;
    void Start()
    {
        kinWrapper = GameObject.Find("KinWrapper").GetComponent<KinWrapper>();
        kinWrapper.Initialize(ListenKin);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void ListenKin(object eventData, string type)
    {
        GameObject.Find("TutorialLog").GetComponent<Text>().text += "\n" + eventData.ToString();
    }
}
