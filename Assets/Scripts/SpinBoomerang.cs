using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinBoomerang : MonoBehaviour
{
    public bool activated;
    
    public float spinSpeed = -1500;
    
    //private int bounces = 0; //Left over from when I wanted the boomerang to bounce once before getting stuck.
    //It didn't work out because I wasn't willing to wait a second for it to bounce. (for example if it was bounced once of a wall
    //then hit the floor on its side it would get stuck.)

    private void Start()
    {
        //bounces = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.Self);
        }
    }
    
    /*
    private void OnCollisionEnter(Collision otherObject)
    {
        if (bounces == 0)
        {
            ++bounces;
        }
        else
        {
            activated = false;
            GetComponent<Rigidbody>().isKinematic = true;
            bounces = 0;
        }
        
    }
    */
    
}
