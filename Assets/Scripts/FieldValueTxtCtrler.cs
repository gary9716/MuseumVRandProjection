using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FieldValueTxtCtrler : MonoBehaviour {

    public Text valueTxt;
    
    public void SetVal(string val)
    {
        valueTxt.text = val;
    }

}
