using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableBoomerang : MonoBehaviour
{
    public Rigidbody boomerangRigidbody;
    public float throwForce = 50;
    
    [SerializeField]
    Transform returnTarget, curve_point;

    private Vector3 old_pos;
    private bool isReturning = false;
    private bool isHeld = true;
    private float time = 0.0f;

    public SpinBoomerang spinController;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Fire1"))
        {
            if (!isReturning)
            {
                ThrowBoomerang();
            }
        }
        if (Input.GetButtonUp("Fire2"))
        {
            if (!isReturning && !isHeld)
            {
                ReturnBoomerang();
            }
        }

        if (isReturning)
        {
            if (time < 1.0f)
            {
                boomerangRigidbody.position = getBQCPoint(time, old_pos, curve_point.position, returnTarget.position);
                boomerangRigidbody.rotation = Quaternion.Slerp(boomerangRigidbody.transform.rotation, returnTarget.rotation, 50 * Time.deltaTime);
                time += Time.deltaTime;
            }
            else
            {
                ResetBoomerang();
            }
        }
    }
    
    void ThrowBoomerang()
    {
        isHeld = false;
        boomerangRigidbody.transform.parent = null;
        boomerangRigidbody.isKinematic = false; //Allows it to be thrown now. Usually it's kinematic.
        boomerangRigidbody.AddForce(Camera.main.transform.TransformDirection(Vector3.forward) * throwForce,ForceMode.Impulse);
        //boomerangRigidbody.AddTorque(boomerangRigidbody.transform.TransformDirection(Vector3.right) * 100, ForceMode.Impulse);
        spinController.activated = true;
    }

    void ReturnBoomerang()
    {
        time = 0.0f;
        old_pos = boomerangRigidbody.position;
        isReturning = true;
        boomerangRigidbody.velocity = Vector3.zero;
        //boomerangRigidbody.isKinematic = true; //Because we are spinning the boomerang now we are stepping on the toes
        //of our original returnBoomerang idea in Update (where position is constantly updated). We'll need to make it kinematic
        //when it returns to our hand.
    }

    void ResetBoomerang()
    {
        spinController.activated = false;
        time = 1.0f;
        isReturning = false;
        boomerangRigidbody.transform.parent = transform;
        boomerangRigidbody.transform.position = returnTarget.position;
        boomerangRigidbody.transform.rotation = returnTarget.rotation;
        isHeld = true;
        boomerangRigidbody.isKinematic = true;
    }
    
    //Create a Bezier Quadratic Curve that returns the axe based on t (time). Though t acts like a percentage measuring
    //a portion of the distance from one point to the other.
    Vector3 getBQCPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }
    
}
