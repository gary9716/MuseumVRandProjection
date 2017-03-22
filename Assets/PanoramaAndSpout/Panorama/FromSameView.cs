using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromSameView : MonoBehaviour {

    [HideInInspector]
    public bool isFollowing = false;
    public Transform target;
    public bool onlyHorizontalRotate;

    private void LateUpdate()
    {
        if(isFollowing)
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
