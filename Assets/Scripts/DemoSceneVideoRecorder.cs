using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSceneVideoRecorder : MonoBehaviour {
    
    public AVProMovieCaptureFromTexture _movieCapture;
    public panoramaController panoramaCtrler;

    public bool recordInLimitedTime;
    public float recordTimeInSecs;

    bool textureExisted = false;

    // Use this for initialization
    void Start () {

        RenderTexture recordingTex = panoramaCtrler.cam.targetTexture;
        if(recordingTex == null)
        {
            Debug.LogWarning("video recording feature is disabled");
            enabled = false;
            if(_movieCapture)
            {
                _movieCapture._captureKey = KeyCode.None;
            }
            return;
        }


        textureExisted = true;
        _movieCapture.SetSourceTexture(recordingTex);

        //CancelCapture : stop recording and delete the file
        //StartCapture : start recording
        //StopCapture : stop recording
        //ResumeCapture : continue
    }

    // Update is called once per frame
    void Update () {
		if(textureExisted)
        {

            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("Start Capturing, fileName:" + _movieCapture.LastFilePath + ",resolution:" + _movieCapture._renderResolution + ",frameRate:" + _movieCapture._frameRate);

                _movieCapture.StartCapture();
            }
            else if(Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("Cancel Capturing");
                _movieCapture.CancelCapture();
            }
            else if(Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("Stop Capturing");
                _movieCapture.StopCapture();
            }
        }

	}
}
