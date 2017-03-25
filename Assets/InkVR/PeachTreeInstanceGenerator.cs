using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PeachTreeInstanceGenerator : MonoBehaviour {
#if UNITY_EDITOR

    public string peachTreesPath = "Assets/InkVR/PeachTreeInstances";
    public GameObject toRuntimeSavedObj;

    // Use this for initialization
    void Start () {
        string prefabName = "tree1";
        //Object emptyObj = GenerateEmptyPrefab(prefabName, peachTreesPath);

        GameObject tempObj = toRuntimeSavedObj;
        if (tempObj == null)
            tempObj = gameObject; //current gameObject attached with this script

        tempObj.name = prefabName;
        PeachTreeLandingPtsCtrler ptlCtrler = tempObj.GetComponent<PeachTreeLandingPtsCtrler>();
        if(ptlCtrler == null)
        {
            Debug.LogWarning("this gameObject should be attached with PeachTreeLandingPtsCtrler");
            return;
        }

        ptlCtrler.GenerateLandingPtsBasedOnBranches();
        
        //PrefabUtility.ReplacePrefab(tempObj, emptyObj, ReplacePrefabOptions.ConnectToPrefab);
    }

    // Update is called once per frame
    void Update () {
		
	}

    Object GenerateEmptyPrefab(string prefabName, string path)
    {
        return PrefabUtility.CreateEmptyPrefab(path + "/" + prefabName + ".prefab");
    }

#endif

}

