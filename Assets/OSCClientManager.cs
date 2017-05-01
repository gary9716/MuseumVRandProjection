using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCClientManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        OSCHandler.Instance.InitOFSpoutAppClient(10000);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.H))
        {
            OSCHandler.Instance.SendMessageToOFSpoutApp<string>("winCtrl", "hide");
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            OSCHandler.Instance.SendMessageToOFSpoutApp<string>("winCtrl", "show");
        }

    }
}
