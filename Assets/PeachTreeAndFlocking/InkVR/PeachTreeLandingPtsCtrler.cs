using System.Collections.Generic;
using UnityEngine;

public class PeachTreeLandingPtsCtrler : MonoBehaviour {

	public bool testInStart;
	public Transform testObj;

	public bool putTestingButterfly;
	public GameObject testButterfly;

	public Transform placeholderPrefab;

	public Transform branchRoot;

	public float basicDist = 0.3f;

	public Transform ptsRoot;
	public List<Landable> landablePts = new List<Landable>();
	public int numPts {
		get {
			return landablePts.Count;
		}
	}
	
	bool landingPtGenerated = false;

	void AddLandingPt(string name, Vector3 pos) {
		Transform pt = Instantiate(placeholderPrefab, pos, Quaternion.identity);
		pt.parent = ptsRoot;
		pt.localEulerAngles = new Vector3(0,Random.Range(0,360),0);
		
		if(putTestingButterfly) {
			GameObject butterflyObj = Instantiate(testButterfly, Vector3.zero, Quaternion.identity);
			butterflyObj.transform.parent = pt;
			butterflyObj.transform.position = pt.position;
			butterflyObj.transform.rotation = pt.rotation;
		}
		
		Landable landable = pt.GetComponent<Landable>();
		if(landable != null)
			landablePts.Add(landable);
	}

	// Use this for initialization
	void Start () {
		if(testInStart) {
			GenerateLandingPtsBasedOnBranches();
			print("total num land pts:" + numPts);
			if(testObj != null)
				SetPtsDirBasedOnObserver(testObj.position);
		}
	}

	public void GenerateLandingPtsBasedOnBranches() {
		if(landingPtGenerated)
			return;

		EdgeCollider2D[] edges = branchRoot.GetComponentsInChildren<EdgeCollider2D>();
		foreach(EdgeCollider2D edge in edges) {
			Vector3 edgeWorldPos = edge.transform.position;
			
			Vector2 localPos = edge.points[0];
			Vector3 lastPtPos = new Vector3(edgeWorldPos.x + localPos.x, edgeWorldPos.y - localPos.y, edgeWorldPos.z);
			AddLandingPt(edge.name + "_" + 0, lastPtPos);
			float accumDist = 0;
			Vector3 prePtPos = lastPtPos;
			for(int i = 1;i < edge.pointCount;i++) {
				localPos = edge.points[i];
				Vector3 worldPos = new Vector3(edgeWorldPos.x + localPos.x, edgeWorldPos.y - localPos.y, edgeWorldPos.z);
				accumDist += Vector3.Distance(worldPos, prePtPos);
				
				while(accumDist > basicDist) {
					accumDist -= basicDist;
					lastPtPos = prePtPos + (worldPos - prePtPos) * Random.value;
					AddLandingPt(edge.name + "_" + i, lastPtPos);
				}
				
				prePtPos = worldPos;
			}

		}

		landingPtGenerated = true;
	}
	
	public void SetPtsDirBasedOnObserver(Vector3 obsPos) {
		Vector3 obsDir = obsPos - transform.position;
		float dotVal = Vector3.Dot(obsDir, transform.up);
		foreach(Landable pt in landablePts) {
			Vector3 eulerAngles = pt.getTrans().localEulerAngles;
			eulerAngles.x = dotVal > 0 ? 0:180;
			pt.getTrans().localEulerAngles = eulerAngles;	
		}
	}

	public Landable GetOneLandablePt() {
		foreach(Landable pt in landablePts) {
			if(pt.isLandable())
				return pt;
		}

		return null;
	}
}
