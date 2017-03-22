using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//original source: http://wiki.unity3d.com/index.php?title=Flocking
//fixed by KT on 2017/03/13
//align facing direction with movement
//add coefficient to steer

public class BoidFlocking : MonoBehaviour
{
	internal BoidController controller;
    public Rigidbody rigid;
    public SphereCollider sphereCollider;

    public Animator animator;

    public enum State {
        none,
        perching,
        approaching,
        landing,
        flocking
    }

    public State preState = State.none;
    public State curState = State.none;

    public Landable landingPt;

    Transform target;

    IEnumerator Start()
	{
        WaitForSeconds waitForFixedSecs = new WaitForSeconds(Random.Range(0.05f, 0.3f));
        
        if(curState == State.none)
            EnterState(State.flocking);

        while (true)
		{
            CalcNewVelocity();
			yield return waitForFixedSecs;
		}
	}
    
    void SetAnimSpeed(float speed) {
        if (animator)
            animator.SetFloat("moveSpeed", speed);
    }

    void CalcNewVelocity() {
        if (controller)
        {
            if(curState == State.flocking) {
                rigid.velocity += steer() * Time.deltaTime;
            }
            else if(curState == State.approaching) {
                Vector3 attractDir = attract();
                rigid.velocity += (attractDir * Mathf.Abs(Vector3.Angle(attractDir, rigid.transform.forward) * 0.8f) * Time.deltaTime);
                rigid.velocity *= Vector3.Distance(target.position, rigid.transform.position) * 0.3f;
            }
            else if(curState == State.landing) {
                rigid.velocity = attract().normalized * Vector3.Distance(target.position, rigid.transform.position);
            }
            else {
                return;
            }

            // enforce minimum and maximum speeds for the boids
            float speed = rigid.velocity.magnitude;
            
            if (speed > controller.maxVelocity)
            {
                rigid.velocity = rigid.velocity * (controller.maxVelocity / speed);
            }
            else if (speed < controller.minVelocity && curState != State.landing)
            {
                rigid.velocity = rigid.velocity * (controller.minVelocity / speed);
            }
            
            SetAnimSpeed(rigid.velocity.magnitude);

        }
        else
        {
            this.enabled = false;
        }
    }

    public void SetTarget(Transform target) {
        this.target = target;
    }

    public void EnterState(State state) {
        if(state == curState)
            return;
        
        preState = curState;
        curState = state;

        if(curState == State.perching) {
            animator.SetBool("isIdle", true);
            SetAnimSpeed(0);
            if(isAvoiding)
                ChangeAvoidMode(false);
            rigid.isKinematic = true;
            rigid.velocity = Vector3.zero;
            landingPt = target.GetComponent<Landable>();
        }
        else if(curState == State.landing) {
            
        }
        else if(curState == State.approaching) {
            ChangeAvoidMode(false);
        }
        else if(curState == State.flocking) {
            animator.SetBool("isIdle", false);
            rigid.isKinematic = false;
            ChangeAvoidMode(true);
        }

    }

    public void ChangeAvoidMode(bool enabled) {
        sphereCollider.enabled = enabled;
    }

    public bool isAvoiding {
        get {
            return sphereCollider.enabled;
        }
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        sphereCollider.radius = controller.separationRadius;
        UpdateState();

        if(rigid.velocity != Vector3.zero)
            rigid.transform.rotation = Quaternion.LookRotation(rigid.velocity);
        
    }

    void UpdateState() {
        if(curState == State.landing) {
            if(target != null && Vector3.Distance(target.position, rigid.transform.position) < 0.05f) {
                EnterState(State.perching);
            }                        
        }
        else if(curState == State.approaching) {
            float distThreshold = rigid.velocity.magnitude * Time.deltaTime;
            if(target != null && Vector3.Distance(target.position, rigid.transform.position) < distThreshold * 1.5f) {
                EnterState(State.landing);
            }
        }
        else if(curState == State.flocking) {
            if(target != null && Vector3.Distance(target.position, rigid.transform.position) < controller.strongAttractDist) {
                EnterState(State.approaching);
            }
        }
    }


    Vector3 attract() {
        Vector3 attract = Vector3.zero;

        if(target != null)
            attract = Vector3.ClampMagnitude(target.position - rigid.transform.position, controller.maxVelocity) * controller.attractFactor;
        
        return attract;
    }

    Vector3 steer()
    {
        //separation is dealed with sphere collider and physics material
        Vector3 cohesion = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        if(controller.numFlockingInstances > 2) {
            cohesion = Vector3.ClampMagnitude(controller.flockCenter - rigid.transform.position, controller.maxVelocity) * controller.cohesionFactor;
            alignment = Vector3.ClampMagnitude(controller.flockVelocity, controller.maxVelocity) * controller.alignFactor;
        }

        Vector3 randomize = Vector3.ClampMagnitude(new Vector3((Random.value * 2) - 1, (Random.value * 2) - 1, (Random.value * 2) - 1), controller.maxVelocity) * controller.randomness;

        return (cohesion + alignment + attract() + randomize);

    }

}