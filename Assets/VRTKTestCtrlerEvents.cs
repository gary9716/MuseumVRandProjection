using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class VRTKTestCtrlerEvents : MonoBehaviour {

    public VRTK_ControllerEvents leftCtrlerEventInfo;
    public VRTK_ControllerEvents rightCtrlerEventInfo;
	// Use this for initialization
	void Start () {
        if (leftCtrlerEventInfo == null)
            leftCtrlerEventInfo = GameObject.FindGameObjectWithTag("VRTKLeftCtrler").GetComponent<VRTK_ControllerEvents>();
        if(rightCtrlerEventInfo == null)
            rightCtrlerEventInfo = GameObject.FindGameObjectWithTag("VRTKRightCtrler").GetComponent<VRTK_ControllerEvents>();
    }
	
	// Update is called once per frame
	void Update () {
        if (leftCtrlerEventInfo.touchpadPressed)
            print("left: touchPad pressed");

        print("left trigger:" + leftCtrlerEventInfo.GetTriggerAxis());

        if (leftCtrlerEventInfo.grabPressed)
            print("left: grab pressed");
        if (leftCtrlerEventInfo.triggerClicked)
            print("left: trigger clicked");
        if (rightCtrlerEventInfo.touchpadPressed)
            print("right: touchPad pressed");
        if (rightCtrlerEventInfo.grabPressed)
            print("right: grab pressed");
        if (rightCtrlerEventInfo.triggerClicked)
            print("right: trigger clicked");
    }
}
