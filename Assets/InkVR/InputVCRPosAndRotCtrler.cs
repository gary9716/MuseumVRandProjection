using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputVCRPosAndRotCtrler : MonoBehaviour {

    InputVCR vcr;

    Vector3 lastPos;
    Quaternion lastRot;

    //Vector3 targetPos;
    //Quaternion targetRot;

    Vector3 nextPos;
    Quaternion nextRot;

    Vector3 srcPos;
    Vector3 srcAngles;
    
    public float damping = 10f; // how fast playback will catch up to recording. Higher = more accurate but less smooth

    Vector3 GetInitPos()
    {
        return vcr.recordLocalInfo ? transform.localPosition : transform.position;
    }

    Quaternion GetInitRot()
    {
        return vcr.recordLocalInfo ? transform.localRotation : transform.rotation;
    }

    void SetPosAndRot(Vector3 pos, Quaternion rot)
    {
        if (vcr.recordLocalInfo)
        {
            transform.localPosition = pos;
            transform.localRotation = rot;
        }
        else
        {
            transform.position = pos;
            transform.rotation = rot;
        }
    }


    void Awake()
    {
        vcr = GetComponent<InputVCR>();

        if (vcr == null)
        {
            this.enabled = false;
            Debug.LogWarning("no inputVCR but attached with InputVCRPosAndRotCtrler");
            return;
        }

        //targetPos = GetInitPos();
        //targetRot = GetInitRot();
        lastPos = GetInitPos();
        lastRot = GetInitRot();
        nextPos = GetInitPos();
        nextRot = GetInitRot();
        
    }

    void Update()
    {
        if (vcr.mode == InputVCRMode.Playback)
        {
            bool teleported = vcr.GetProperty("teleported") == "1";
            string posString = vcr.GetProperty("position");
            if (!string.IsNullOrEmpty(posString))
                nextPos = InputVCR.ParseVector3(posString);
            string rotString = vcr.GetProperty("rotation");
            if (!string.IsNullOrEmpty(rotString))
                nextRot = Quaternion.Euler(InputVCR.ParseVector3(rotString));

            if (!teleported)
            {
                srcPos = vcr.recordLocalInfo ? transform.localPosition : transform.position;
                srcAngles = vcr.recordLocalInfo ? transform.localEulerAngles : transform.eulerAngles;

                // will try to guess next target position between network frames. 
                Vector3 posChange = srcPos - lastPos;
                Quaternion rotChange = Quaternion.FromToRotation(lastRot.eulerAngles, srcAngles);

                nextPos += posChange;
                nextRot *= rotChange;
                
                Vector3 resultPos = Vector3.Lerp(srcPos, nextPos, Time.deltaTime * damping);
                Quaternion resultRot = Quaternion.Lerp(vcr.recordLocalInfo ? transform.localRotation : transform.rotation, nextRot, Time.deltaTime * damping);

                SetPosAndRot(resultPos, resultRot);
            }
            else
            {
                SetPosAndRot(nextPos, nextRot);
            }

            /*
            // update target pos if location was recorded this frame
            if (!string.IsNullOrEmpty(posString))
                targetPos = nextPos;
            if (!string.IsNullOrEmpty(rotString))
                targetRot = nextRot;
            */

            lastPos = vcr.recordLocalInfo? transform.localPosition : transform.position;
            lastRot = vcr.recordLocalInfo? transform.localRotation : transform.rotation;
        }
        else
        {
            lastPos = nextPos = vcr.recordLocalInfo? transform.localPosition : transform.position;
            lastRot = nextRot = vcr.recordLocalInfo? transform.localRotation : transform.rotation;
        }
    }
}
