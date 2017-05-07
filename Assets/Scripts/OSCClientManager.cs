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
            sendHideProjectionMsg();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            sendShowProjectionMsg();
        }

    }

    public void sendShowProjectionMsg()
    {
        OSCHandler.Instance.SendMessageToOFSpoutApp<string>("winCtrl", "show");
    }

    public void sendHideProjectionMsg()
    {
        OSCHandler.Instance.SendMessageToOFSpoutApp<string>("winCtrl", "hide");
    }

    public void sendExitMsg()
    {
        OSCHandler.Instance.SendMessageToOFSpoutApp<string>("appCtrl", "exit");
    }

}
