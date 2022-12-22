using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.UI;

public class ThrowBoomerangAlt : MonoBehaviour
{
    [Header("Body References")] 
    [SerializeField] private Transform hand;
    [SerializeField] private Transform weapon;
    public Rigidbody weaponRigidbody;
    [Space]
    [Header("Boomerang Related")]
    [SerializeField] private float throwForce = 50;
    [SerializeField] private Transform curvePoint;
    [SerializeField] private TrailRenderer weaponTrail;
    [Space]
    [Header("Camera Effects")]
    [SerializeField] private CinemachineImpulseSource screenShakeSignal;
    [SerializeField] private float cameraZoomOffset = 0.3f;
    public CinemachineFreeLook aimCamera;
    [Space]
    
    [SerializeField] private MovementInput input;

    [Space] [SerializeField] private Image aimingReticle;
    
    private SpinBoomerang spinController;
    
    private Vector3 origPosition;
    private Vector3 origRotation;
    private Vector3 posAtRecall;

    private Animator anim;
    
    private float returnTime = 0.0f;
    private bool isReturning = false;
    private bool isHeld = true;
    private bool isAiming = false;
    private bool isWalking = false;
    
    void Start()
    {
        origPosition = weapon.localPosition;
        origRotation = weapon.localEulerAngles;

        anim = this.GetComponent<Animator>();
        spinController = weapon.GetComponent<SpinBoomerang>();
        
        aimingReticle.DOFade(0, 0);
    }
    // Update is called once per frame
    void Update()
    {
        if (isAiming)
        {
            input.RotateToCamera(transform);
        }

        isWalking = input.Speed > 0;
        anim.SetBool("isWalking",isWalking);
        if (Input.GetButtonDown("Fire1"))
        {
            if (isAiming)
            {
                StartAiming(false, false, 0f); //Stop aiming (on account of the fact that axe is thrown).
                //but keep the camera in the same position. We do it here because WeaponThrow() is called by the Animator in the
                //AnimationEvent attached to the "Throw" animation.
                //In short, there's a delay between this button press and the call to WeaponThrow() that can mess up the
                //reticle.
                anim.SetTrigger("startThrow"); //I can only hope that the animation doesn't snap in the middle.
            }

            /*
            if (!isHeld && !isReturning)
            {
                anim.SetTrigger("startPullAnim");
                ReturnBoomerang();
            }
            */
        }
        if (Input.GetButtonDown("Fire2"))
        {
            
            if (!isReturning && !isHeld) //If the weapon is not returning already nor is it held.
            {
                anim.SetTrigger("startPullAnim");
                ReturnBoomerang();
            }
            
            if (isHeld && !isAiming) //If we are holding the boomerang (and an extra check to ensure we are not already aiming)
            {
                StartAiming(true,true,0f);
                //StartAiming(false,true,0f);
            }
            
        }
        
        if (Input.GetButtonUp("Fire2") && isHeld && isAiming) //We only cancel our aim if we are already aiming.
        {
            //Holding the weapon and the right click has been released
            //Means we want to stop aiming. So let's get to that.
            StartAiming(false,true,0f);
        }
        
        /*
        if (Input.GetButtonDown("Fire2") && isHeld) //Aim when the mouse button goes down. This is a pseudo hold and release
            //control.
        {
            StartAiming(true,true,0f);
        }
        */
        
        //The control has now been transferred over the right click instead of being a part of the left click.

        if (isReturning)
        {
            if (returnTime < 1.0f)
            {
                weaponRigidbody.position = GetQBCPoint(returnTime, posAtRecall, curvePoint.position, hand.position);
                weaponRigidbody.rotation = Quaternion.Slerp(weaponRigidbody.transform.rotation, 
                    Quaternion.Euler(origRotation), 50 * Time.deltaTime);
                returnTime += Time.deltaTime;
            }
            else
            {
                ResetBoomerang();
            }
        }
    }
    
    void StartAiming(bool active, bool changeCamera, float delay)
    {
        //Set the camera to the virtual camera
        //Makes the aiming cursor appear
        //Rotate the character to where the camera is facing.
        
        //Okay, so hard setting the rotation doesn't work. The guy isn't turning.
        //this.transform.rotation = Quaternion.Euler( 0, Camera.main.transform.eulerAngles.y, 0);
        
        /*
        if (isWalking) //Don't aim if you are walking.
            return;
        */
        
        isAiming = active;
        anim.SetBool("isAiming", isAiming);
        
        float fade = active ? 1 : 0;
        aimingReticle.DOFade(fade, .2f);
        
        if (!changeCamera) //If changeCamera is false then we do not want to zoom-in/out the camera. That's all.
            return;
        
        float newAim = active ? cameraZoomOffset : 0;
        float originalAim = !active ? cameraZoomOffset : 0;
        DOVirtual.Float(originalAim, newAim, .5f, CameraOffset).SetDelay(delay);
        
        
    }
    
    public void WeaponThrow() //The weapon throw is attached to an animation event because it needs to start a certain time
    //in the animation.
    {
        //Set trail renderer emitting here.
        weaponRigidbody.transform.parent = null;
        weaponRigidbody.isKinematic = false;
        //But if it's discrete our chances of throwing it through an object goes up drastically.
        weapon.transform.position += transform.right/5; //Get it clear from colliding with our hand
        //weaponRigidbody.AddForce(Camera.main.transform.forward * throwForce,ForceMode.Impulse);
        weaponRigidbody.AddForce(Camera.main.transform.TransformDirection(Vector3.forward) * throwForce,ForceMode.Impulse);

        spinController.activated = true;
        isHeld = false;
        
        isAiming = false;
        anim.SetBool("isAiming", isAiming); //For now, aiming does not take control of the camera in any way.
        //but once it does we need to replace our set bool path.
        
        weaponTrail.emitting = true;
    }

    public void ReturnBoomerang() //The movement of the boomerang from it's current position back to the hand will be
    //settled in update using GetQBCPoint.
    {
        returnTime = 0.0f; //Reset our return time to properly hand it over to GetQBCPoint in Update().
        posAtRecall = weaponRigidbody.position;
        //weaponRigidbody.isKinematic = true;
        isReturning = true;
    }
    
    //QBC stands for "Quadratic Bezier Curve" it's one of those cool looking curves that can be created when given
    //a source and destination with a third point that the line between the two will be lightly pulled toward to make
    //a curve.
    //t is time
    //p0 is where the weapon was at time of recall. (Source point)
    //p1 is the third point that influences the sharpness of the curve by "pulling" on the line between points 1 and 2.
    //p2 is the destination point (our hand in this case)
    public Vector3 GetQBCPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
    
    public void ResetBoomerang()
    {
        isReturning = false;
        isHeld = true;
        
        weapon.parent = hand; //Forgetting to set the parent to hand will set the localPosition to world origin point.
        //weaponRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        spinController.activated = false;
        weapon.localPosition = origPosition;
        weapon.localEulerAngles = origRotation;
        
        anim.SetTrigger("startCatchAnim"); //This false statement transitions the pull animation into the "Catch" state
        
        //Remove the weapon trail.
        weaponTrail.emitting = false;
        
        weaponRigidbody.isKinematic = true;

        //Shake the screen
        screenShakeSignal.GenerateImpulse(Vector3.right);
        
        StartAiming(false,true,0f); //Return the camera back to normal.
        
        //Most likely localPosition is 0;
    }
    
    void CameraOffset(float offset)
    {
        aimCamera.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(offset, 0f, 0);
        aimCamera.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(offset, 0f, 0);
        aimCamera.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(offset, 0f, 0);
    }
}
