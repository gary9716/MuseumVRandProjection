using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToTarget : MonoBehaviour {

    public bool snapPos;
    public bool snapRot;
    public Transform target;

    Quaternion rotOffset = Quaternion.identity;
    Vector3 posOffset = Vector3.zero;


    private void Start()
    {
        if(target != null)
        {
            if (snapPos)
                posOffset = transform.position - target.position;

            if (snapRot)
                rotOffset = Quaternion.FromToRotation(target.eulerAngles, transform.eulerAngles);

        }

    }

    // Update is called once per frame
    void Update () {
		
        if(target != null)
        {

            if (snapPos)
            {
                transform.position = target.position + posOffset;
            }

            if (snapRot)
            {
                transform.rotation = target.rotation * rotOffset;
            }

        }

	}
}
