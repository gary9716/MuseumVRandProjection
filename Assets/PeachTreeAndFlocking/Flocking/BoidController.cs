using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//original source: http://wiki.unity3d.com/index.php?title=Flocking
//fixed by KT on 2017/03/17
//add coefficient

/// <summary>
/// these define the flock's behavior
/// </summary>
public class BoidController : MonoBehaviour
{
	public float minVelocity = 5;
	public float maxVelocity = 20;
	public int flockSize = 120;
	public BoidFlocking prefab;
	public Transform target;
    
    internal Vector3 flockCenter;
	internal Vector3 flockVelocity;

    [Range(-20, 20)]
    public float cohesionFactor = 0.4f;

    [Range(0, 10)]
    public float separationRadius = 3f;

    [Range(0, 20)]
    public float alignFactor = 0.5f;

    [Range(0, 20)]
    public float attractFactor = 1f;

    [Range(0, 5)]
    public float randomness = 1f;

    public float strongAttractDist = 0.3f;
	public float perchingDist = 0.01f;
    List<BoidFlocking> boids = new List<BoidFlocking>();

	public bool allBoidsPerching {
		get {
			return numFlockingInstances == 0;
		}
	}

	public PeachTreeLandingPtsCtrler perchingTree;
	public PeachTreesManager peachTreesManager;

	bool canBeTriggered = true;

	public void Triggered() {
		if(!canBeTriggered || !allBoidsPerching) 
			return;

		canBeTriggered = false;

		if(peachTreesManager != null)
			peachTreesManager.FlyToAnotherTree();

		canBeTriggered = true;
	}

	void Start()
	{
        
		
	}

	public void RandomGenerateBoidInsideCollider(BoidFlocking.State initState) {
		Collider collider = GetComponent<Collider>();
		Debug.Assert(collider != null, "no collider on this gameObject");

		boids.Clear();
		for (int i = 0; i < flockSize; i++)
		{
			BoidFlocking boid = Instantiate(prefab, transform.position, transform.rotation) as BoidFlocking;
			boid.transform.parent = transform;
			boid.transform.localPosition = new Vector3(
							Random.value * collider.bounds.size.x,
							Random.value * collider.bounds.size.y,
							Random.value * collider.bounds.size.z) - collider.bounds.extents;
			boid.controller = this;
			boid.EnterState(initState);
			boids.Add(boid);
		}
	}

	public void PutBoidsOnTree(PeachTreeLandingPtsCtrler peachTree) {
		boids.Clear();
		foreach (Landable landable in peachTree.landablePts)
		{
			Transform landingPtTrans = landable.getTrans();
			BoidFlocking boid = Instantiate(prefab, landingPtTrans.position, landingPtTrans.rotation) as BoidFlocking;
			boid.transform.parent = transform;
			
			boid.controller = this;
			landable.TargetBy(boid);

			boid.EnterState(BoidFlocking.State.perching);
			boids.Add(boid);

		}
		perchingTree = peachTree;
	}

	public void FlyToTree(PeachTreeLandingPtsCtrler peachTree, bool scattered = true) {
		
		if(peachTree == perchingTree)
			return;
		
		foreach(Landable landable in perchingTree.landablePts) {
			landable.ReleaseAll();
		}

		perchingTree = peachTree;

		if(scattered) 
			cohesionFactor = -Mathf.Abs(cohesionFactor);
		else
			cohesionFactor = Mathf.Abs(cohesionFactor);
		
		foreach(BoidFlocking boid in boids) {
			Landable landable = peachTree.GetOneLandablePt();
			if(landable == null) {
				Debug.Log("not enough landing pts QAQ");
				break;
			}
			landable.TargetBy(boid);
			boid.EnterState(BoidFlocking.State.flocking);
		}

		if(cohesionFactor < 0)
			Invoke("RecoverCoherent", 1);
	}

	void RecoverCoherent() {
		cohesionFactor = Mathf.Abs(cohesionFactor);
	}



	[HideInInspector]
	public int numFlockingInstances = 0;

	void Update()
	{
		Vector3 center = Vector3.zero;
		Vector3 velocity = Vector3.zero;
		numFlockingInstances = 0;
		foreach (BoidFlocking boid in boids)
		{
			if(boid.curState == BoidFlocking.State.flocking) {
				center += boid.transform.position;
				velocity += boid.rigid.velocity;
				numFlockingInstances++;
			}
		}

		if(numFlockingInstances > 0) {
			flockCenter = center / numFlockingInstances;
			flockVelocity = velocity / numFlockingInstances;
		}

    }

}

public interface Landable {
	void TargetBy(BoidFlocking boid);

	void Release(BoidFlocking boid);
	void ReleaseAll();

	bool isLandable();

	Transform getTrans();
}