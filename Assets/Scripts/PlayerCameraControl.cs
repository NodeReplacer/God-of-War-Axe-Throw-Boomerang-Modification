using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCameraControl : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = Quaternion.Euler( 0, Camera.main.transform.eulerAngles.y, 0);
        //this.transform.rotation = Camera.main.transform.rotation;
    }

}
