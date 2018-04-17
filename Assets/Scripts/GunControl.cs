﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunControl : NetworkBehaviour {
    public GameObject projectile, projectileParent;
    public float jumpForce, groundedCheckDist, deadZoneThreshold, walkSpeed, airWalkSpeed, airWalkSpeedThreshold, percentVelocityLossPerSecond, explosionRadius, explosionForce, fireRate;
    public int numUpdatesToIgnoreGroundedCheck = 5;
    public SpriteRenderer rocket1, rocket2, rocket3;
    public Color active, inactive;
    private float timeSinceLastProjectile;
    private int groundedCheckReset;
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private int numRocketsLeft = 3;
    private bool hasFired = false;
    private GunInput input;
	// Use this for initialization
	void Start () {
        groundedCheckReset = numUpdatesToIgnoreGroundedCheck;
        rb = GetComponent<Rigidbody2D>();
        timeSinceLastProjectile = fireRate;
        rocket1.color = active;
        rocket2.color = active;
        rocket3.color = active;
        input = GetComponent<GunInput>();
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().toFollow = this.gameObject;
        
    }
	
	// Update is called once per frame
	void Update () {

        if (!isLocalPlayer)
        {
            return;
        }

        rb.velocity = (1 - Mathf.Clamp(percentVelocityLossPerSecond, 0, 1)) * rb.velocity;
        //Debug.Log(Vector2.Distance(new Vector2(), new Vector2(Input.GetAxis("Joystick_2_x"), Input.GetAxis("Joystick_2_y")))+"\n" + new Vector2(Input.GetAxis("Joystick_2_x"), Input.GetAxis("Joystick_2_y")).magnitude);
        
        if (input.inputChange(deadZoneThreshold))//use circle and own deadzone calc to make circular deadzone
        {
            transform.rotation = Quaternion.Euler(input.getRotation());
        }
        //manage grounded state via raycast down. Also a timer in terms of calls to this function that is the cooldown for doing that raycast check
        //prevents player being allowed to double jump due to being deemed grounded the frame after jumping and still being close to the ground
        RaycastHit2D groundedCheck = Physics2D.Raycast((Vector2)(transform.position), Vector2.down, groundedCheckDist, layerMask: 13);
        if (!isGrounded){
            if (--groundedCheckReset <= 0 && groundedCheck.collider != null) {
               // Debug.Log(test.collider.gameObject.name);
                isGrounded = true;
                rocket1.color = active;
                rocket2.color = active;
                rocket3.color = active;
                numRocketsLeft = 3;
            }
        }
        //the jump itself
        if (isGrounded && input.getJump())
        {
            isGrounded = false;
            groundedCheckReset = numUpdatesToIgnoreGroundedCheck;
            rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
        }

        float xAxis = input.getXAxis();
        //left right movement both for ground and in air
        if (isGrounded)
        {
            //determine if input at all
            if(xAxis != 0)
            {
                //right
                if (xAxis > 0)
                {
                    rb.AddForce(walkSpeed * Vector2.right, ForceMode2D.Force);
                }
                //left
                else
                {
                    rb.AddForce(walkSpeed * Vector2.left, ForceMode2D.Force);
                }
            }
        }
        else
        {
            //determine if input at all
            if (xAxis != 0)
            {
                //right
                if (xAxis > 0)
                {
                    rb.AddForce(airWalkSpeed * Vector2.right, ForceMode2D.Force);
                }
                //left
                else
                {
                    rb.AddForce(airWalkSpeed * Vector2.left, ForceMode2D.Force);
                }
            }
        }
        //managing fireRate stuff
        if(timeSinceLastProjectile < fireRate)
        {
            timeSinceLastProjectile += Time.deltaTime;
        }
        //shooting projectiles
        if (input.getFire() && numRocketsLeft > 0)
        {
            if (timeSinceLastProjectile >= fireRate && !hasFired)
            {
                GameObject rocket = Instantiate(projectile);
                rocket.transform.position = this.transform.position;

                projectileController pC = rocket.GetComponent<projectileController>();
                    
                pC.Fire(new Vector2(Mathf.Cos(Mathf.Deg2Rad * (this.transform.rotation.eulerAngles.z - 90)), Mathf.Sin(Mathf.Deg2Rad * (this.transform.rotation.eulerAngles.z - 90))));
            
                hasFired = true;
                //Debug.Log("angle: " + (this.transform.rotation.eulerAngles.z - 90) + "\nexpanded vector:" + new Vector2(Mathf.Cos(Mathf.Deg2Rad * (this.transform.rotation.eulerAngles.z - 90)), Mathf.Sin(Mathf.Deg2Rad * (this.transform.rotation.eulerAngles.z - 90))));
                RaycastHit2D test = Physics2D.Raycast((Vector2)(transform.position), new Vector2(Mathf.Cos(Mathf.Deg2Rad * (this.transform.rotation.eulerAngles.z - 90)), Mathf.Sin(Mathf.Deg2Rad * (this.transform.rotation.eulerAngles.z - 90))), distance: Mathf.Infinity, layerMask: 13);
                if (!isGrounded)
                {
                    
                    if (numRocketsLeft == 1)
                        rocket1.color = inactive;
                    if (numRocketsLeft == 2)
                        rocket2.color = inactive;
                    if (numRocketsLeft == 3)
                        rocket3.color = inactive;

                }
                if (test.collider != null)
                {

                    //Debug.Log(test.collider.gameObject.name + "   " + Vector2.Distance((Vector2)(test.transform.position), (Vector2)transform.position));
                    //projectile.transform.position = new Vector3(test.point.x, test.point.y, projectile.transform.position.z);
                    timeSinceLastProjectile = 0;

                    if (Vector2.Distance(test.point, (Vector2)transform.position) < explosionRadius && numRocketsLeft > 0)
                    {
                        if (isGrounded)
                            numRocketsLeft += 1;
                        isGrounded = false;
                        groundedCheckReset = numUpdatesToIgnoreGroundedCheck;
                        rb.AddForce(-explosionForce * (test.point - (Vector2)this.transform.position).normalized, ForceMode2D.Impulse);
                    }

                }
                if(!isGrounded)
                numRocketsLeft -= 1;
            }
        }else
        {
            hasFired = false;
        }
    }
}
