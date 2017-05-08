using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCSender : MonoBehaviour {

    public OSCClientManager oscClient;

	// Use this for initialization
	void Start () {
        if (oscClient == null)
            oscClient = FindObjectOfType<OSCClientManager>();
    }
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape))
        {
            oscClient.sendExitMsg();
        }
        else if(Input.GetKeyDown(KeyCode.F1))
        {
            OSCHandler.Instance.SendMessageToOFSpoutApp<string>("appCtrl", "toggleDebug");
        }

        for (int i = 0; i < keyCodes.Length; i++)
        {
            if (Input.GetKeyDown(keyCodes[i]))
            {
                int numberPressed = i;
                //Debug.Log(numberPressed);
                

            }

        }
    }

    private KeyCode[] keyCodes = {
         KeyCode.Alpha0,
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };
}
