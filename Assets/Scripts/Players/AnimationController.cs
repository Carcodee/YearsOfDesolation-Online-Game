using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class AnimationController : NetworkBehaviour
{
    NetworkAnimator networkAnimator;
    
    float aimAnimation;
    public float networkAimAnimation;
    public float networkXMovement;
    public float networkYMovement;
    public bool networkIsSprinting=false;
    public bool networkIsCrouching=false;
    public float networkSlidingTime;

    public float slidingTimer=0f;

    void Start()
    {
        if (IsOwner)
        {
            GetReferences();

        }
    }

    void Update()
    {

        if (IsOwner)
        {
            //normal animator
            MovementAnimation();
            AimAnimation();
            CrouchAndSprint();
            CrouchAnim();
            SetSprintAnim();
        }

    }
    private void FixedUpdate()
    {
  
    }

    void GetReferences()
    {
        networkAnimator = GetComponent<NetworkAnimator>();

    }


    void MovementAnimation()
    {

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        networkAnimator.Animator.SetFloat("X", x);
        networkAnimator.Animator.SetFloat("Y", y);
    }

    public void SetSprintAnim()
    {
        networkAnimator.Animator.SetBool("Sprint", networkIsSprinting);
        
    }
    public void CrouchAnim()
    {

       networkAnimator.Animator.SetBool("Crouch", networkIsCrouching);

    }
    public void CrouchAndSprint()
    {
        if(networkIsCrouching && networkIsSprinting)
        {
            SetSlidingTimer(Time.deltaTime);
            if (networkSlidingTime>= slidingTimer)
            {
                networkIsCrouching = false;
                slidingTimer = 0f;
            }
            return;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            networkIsSprinting = true;

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                //slide
                networkIsCrouching = true;

            }
            return;
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            networkIsCrouching = true;
            networkIsSprinting = false;

            return;
        }
        networkIsCrouching = false;
        networkIsSprinting = false;
    }
    void AimAnimation() {
 
        if (Input.GetKey(KeyCode.Mouse1))
        {
            aimAnimation += Time.deltaTime * 5;
        }
        else
        {
            aimAnimation -= Time.deltaTime * 5;
        }
            aimAnimation = Mathf.Clamp(aimAnimation, 0, 1);
            float LerpedAnim = Mathf.Clamp(Mathf.Lerp(0, 1, aimAnimation), 0, 1);
            networkAnimator.Animator.SetFloat("Aiming", aimAnimation);

    }


    public void SetAimAnimation(float aimAnimation)
    {
        networkAnimator.Animator.SetFloat("Aiming", aimAnimation);

    }

    //Sliding
    public void SetSlidingTimer(float timeStep)
    {
        if (networkSlidingTime > slidingTimer)
        {
            networkSlidingTime = timeStep;
        }
        else
        {
            networkSlidingTime += timeStep;
        }
        
 
    }
    

}


