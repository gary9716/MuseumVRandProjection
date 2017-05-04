using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromSameView : MonoBehaviour {

    public bool autoFollowAfterEnabled;

    [HideInInspector]
    public bool isFollowing = false;
    public Transform target;
    public bool onlyHorizontalRotate;


    private void OnEnable()
    {
        if(autoFollowAfterEnabled)
            isFollowing = true;
    }

    private void LateUpdate()
    {
        if(isFollowing && target != null)
        {
            transform.position = target.position;
            if (onlyHorizontalRotate)
            {
                Vector3 eulerAngles = target.rotation.eulerAngles;
                eulerAngles.x = 0;
                eulerAngles.z = 0;
                transform.rotation = Quaternion.Euler(eulerAngles);
            }
            else
                transform.rotation = target.rotation;
        }
        
    }
}
