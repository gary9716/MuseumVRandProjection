using UnityEngine;
using System.Collections;

public class captureScreenShot : MonoBehaviour {

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            print("captured!!");
            Application.CaptureScreenshot("Screenshot.png");
        }
        
    }

}
