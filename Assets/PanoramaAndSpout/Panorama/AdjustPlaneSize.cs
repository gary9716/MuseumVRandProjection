using UnityEngine;
using System.Collections;

public class AdjustPlaneSize : MonoBehaviour {

    public Transform alignedPlane;

	// Use this for initialization
	void Start () {
        Material mat = GetComponent<MeshRenderer>().material;
        Texture tex = mat.mainTexture;
        transform.localScale = new Vector3(1 * tex.width/((float) tex.height), 1, 1);
        if(alignedPlane)
        {
            transform.localPosition = alignedPlane.localPosition + alignedPlane.localScale.x * 10f * new Vector3(1, 0, 0);
        }
    }
	
}
