using UnityEngine;
using System.Collections;

public class AdjustPlaneSize : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Material mat = GetComponent<MeshRenderer>().material;
        Texture tex = mat.mainTexture;
        transform.localScale = new Vector3(1 * tex.width/((float) tex.height), 1, 1);
        
    }
	
}
