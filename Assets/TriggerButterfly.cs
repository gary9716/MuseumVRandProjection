using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerButterfly : MonoBehaviour {

    public BoidController boidCtrler;

	// Use this for initialization
	void Start () {
        if (boidCtrler == null)
            boidCtrler = FindObjectOfType<BoidController>();
	}
	
    private void OnTriggerEnter(Collider other)
    {
        if (boidCtrler != null && boidCtrler.triggerableTag(other.tag))
        {
            print("triggered from root");
            boidCtrler.Triggered();
        }
    }
}
