using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSceneUIManager : MonoBehaviour {

    public string[] appModeDisplayValues;
    public string[] vcrStateDisplayValues;

    public FieldValueTxtCtrler appModeTxtCtrler; //display current mode
    public FieldValueTxtCtrler vcrStateTxtCtrler; //display vcr state

    // Use this for initialization
    void Start () {
		if(appModeTxtCtrler == null)
            appModeTxtCtrler = GameObject.Find("ModeText").GetComponent<FieldValueTxtCtrler>();
        if(vcrStateTxtCtrler == null)
            vcrStateTxtCtrler = GameObject.Find("VCRText").GetComponent<FieldValueTxtCtrler>();
    }
	
    public void SetAppModeTxt(AppMode appMode)
    {
        if (appModeTxtCtrler)
            appModeTxtCtrler.SetVal(appModeDisplayValues[(int)appMode]);
    }

    public void SetVCRStateTxt(InputVCRMode vcrMode)
    {
        if (vcrStateTxtCtrler)
            vcrStateTxtCtrler.SetVal(vcrStateDisplayValues[(int)vcrMode]);
    }
}
