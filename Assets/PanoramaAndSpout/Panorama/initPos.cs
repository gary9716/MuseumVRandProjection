using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class initPos : MonoBehaviour {
    void Awake()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
