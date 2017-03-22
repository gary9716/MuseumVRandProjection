using UnityEngine;

public class LandingSpotController : MonoBehaviour, Landable {

	public BoidFlocking boid;
	public Collider touchDetector;

	void Start() {
		touchDetector = GetComponent<Collider>();
		if(touchDetector != null) {
			touchDetector.isTrigger = true;
		}		
	}

	public void TargetBy(BoidFlocking boid) {
		boid.SetTarget(transform);
		this.boid = boid;
	}
	public void Release(BoidFlocking boid){
		Debug.Assert(boid == this.boid);
		boid.landingPt = null;
		this.boid = null;
	}
	public void ReleaseAll(){
		Release(boid);
	}

	public bool isLandable() {
		return boid == null;
	}

	public Transform getTrans() {
		return transform;
	}

	/// <summary>
	/// OnTriggerEnter is called when the Collider other enters the trigger.
	/// </summary>
	/// <param name="other">The other Collider involved in this collision.</param>
	void OnTriggerEnter(Collider other)
	{
		if(boid != null && boid.landingPt == (Landable)this && other.tag == "Hand") {
			print("triggered");
			boid.controller.Triggered();
		}
	}
}
