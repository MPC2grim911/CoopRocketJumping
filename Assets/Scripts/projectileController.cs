﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class projectileController : NetworkBehaviour
{

	private Vector2 direction;
	public float speed;
	public float minColliderSize;
	public float maxColliderSize;
    public float minExplosionSize;
    public float maxExplosionSize;
	public float colliderSizeDelta;
    public float explosionSizeDelta;
	public float minForce;
	public float maxForce;
	public float forceDelta;
	private float currentForce;
	private float currentColliderSize;
    private float currentExplosionSize;
	private bool fired = false;
    public GameObject explosion;
    [HideInInspector]
    public Transform playerWhoFiredThis;

	// Use this for initialization
	void Start () {
		Physics2D.IgnoreLayerCollision(8, 9, true);
	}
	
	// Update is called once per frame
	void Update () {
		if (fired)
		{
			transform.position += new Vector3(speed * direction.x, speed * direction.y, 0);
			if (currentForce + forceDelta <= maxForce)
				currentForce += forceDelta;
			if (currentColliderSize + colliderSizeDelta <= maxColliderSize)
				currentColliderSize += colliderSizeDelta;
            if (currentExplosionSize + explosionSizeDelta <= maxExplosionSize)
                currentExplosionSize += explosionSizeDelta;
			transform.localScale = new Vector3(currentColliderSize,currentColliderSize,1);
			//GetComponent<CircleCollider2D>().radius = currentSize;
		}
	}

	public void Fire(Vector2 dir)
	{
		this.gameObject.SetActive(true);
		direction = dir;
		currentColliderSize = minColliderSize;
        currentExplosionSize = minExplosionSize;
        currentForce = minForce;

        transform.localScale = new Vector3(currentColliderSize, currentColliderSize, 1);
        transform.position += new Vector3(speed * direction.x, speed * direction.y, 0);
		fired = true;
	}

	void OnCollisionEnter2D(Collision2D col)
	{
        Debug.Log("\n\n");
        Debug.Log("hit "  + col.gameObject.name);
        Debug.Log("\n\n");
        if (col.otherCollider.transform != playerWhoFiredThis)
        {
            if (fired && col.gameObject.tag == "Player")
            {
                Debug.Log("Trying to push " + col.gameObject.name + "\nwith a rocket force of " + currentForce);
                col.gameObject.transform.GetComponent<GunControl>().applyRocketForceToSelf(currentForce, (Vector2)(this.transform.position));
            }

            if (fired && col.gameObject.tag != "Player")
            {

                ContactPoint2D[] contact = new ContactPoint2D[16];// could hold many points of contact so average all to one center point
                Vector2 centerPointOfCollision = new Vector2();
                int i = col.GetContacts(contact);
                int totalNumPointsContacted = i;
                Debug.Log(i + " point(s) of contact found.");
                for (; i > 0; --i)
                {
                    Debug.Log("Contact #" + i + " = " + contact[i - 1].point);
                    centerPointOfCollision += contact[i - 1].point;
                }
                centerPointOfCollision /= totalNumPointsContacted;
                Debug.Log("Average Point of all contacts = " + centerPointOfCollision);
                i = 0;
                Collider2D[] hitColliders = new Collider2D[16];
                Collider2D coll2d;
                ContactFilter2D filter = new ContactFilter2D();//default filter
                Physics2D.OverlapCircle(centerPointOfCollision, currentExplosionSize, filter, hitColliders);
                while (i < hitColliders.Length && hitColliders[i])
                {
                    coll2d = hitColliders[i];
                    Debug.Log(hitColliders[i].gameObject.name);
                    if(coll2d.tag == "Player")
                    {
                        coll2d.gameObject.transform.parent.GetComponent<GunControl>().applyRocketForceToSelf(currentForce, centerPointOfCollision);
                    }
                    ++i;
                }
                Debug.Log("Num gameObjects within " + currentExplosionSize + " units of centerPoint = " + (i - 1));//i-1 b/c the projectile counts itself
                //creating an explosion object at the point of collision of the size of the explosion
                GameObject temp = Instantiate(explosion, transform.position, Quaternion.identity);
                temp.transform.localScale = new Vector3(2*currentExplosionSize, 2*currentExplosionSize, 1);
                
                fired = false;
                Destroy(this.gameObject);
            }
        }        
	}
}
