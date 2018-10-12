using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ballAutoMovement : MonoBehaviour {

    private float time;
    private Rigidbody rb;

    public float timeInterval;

	// Use this for initialization
	void Start () {
        time = Time.time;
        rb = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        float currentTime = Time.time;
        if (currentTime - time > timeInterval)
        {
            Vector3 newVelocity =
            new Vector3(Random.Range(-10.0f, 10.0f),
                                    0.0f,
                                    Random.Range(-10.0f, 10.0f));

            rb.velocity = newVelocity;
            time = Time.time;
        }

       
	}
}
