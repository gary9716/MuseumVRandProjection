using UnityEngine;
using System.Collections;

public class setHFOV : MonoBehaviour {

    Camera cam;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        float vFOVRad = cam.fieldOfView * Mathf.Deg2Rad;
        float hFOVDeg = 2 * Mathf.Rad2Deg * Mathf.Atan(cam.aspect * Mathf.Tan(vFOVRad / 2));
        print("deg:" + hFOVDeg);
	}
}
