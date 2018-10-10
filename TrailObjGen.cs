using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailObjGen : MonoBehaviour {

    static int trailNum;
    public GameObject bullet;
    public GameObject trail;
	// Use this for initialization
	void Start () {
        trailNum = 0;
	}
	
	
	void FixedUpdate () {
        if (Input.GetMouseButtonDown(0) && Time.timeScale != 0)
        {
            trailNum++;

            GameObject newBullet = Instantiate(bullet);
            GameObject newTrail = Instantiate(trail);
            TrailEffect trailEffect =  newTrail.GetComponent<TrailEffect>();
            trailEffect.player = newBullet;


            Rigidbody rb = newBullet.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(-8.0f, 8.0f, 0);
        }
	}

    
}
