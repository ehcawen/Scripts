using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private Rigidbody rb;
    private float currentTime=0;

    public float speed;
    public float upSpeed;
    public float downSpeed = 10.0f;
    public float power = 10.0f;

    private Camera mainCamera;

    float time;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        
    }

    private void FixedUpdate()
    {

        

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        Vector3 drop = new Vector3(0.0f, -downSpeed, 0.0f);
        Vector3 movement = new Vector3(moveHorizontal,0.0f,moveVertical);

        

        if (Input.GetKeyDown("space"))
        {
            launchPlayer();
        }else if (Input.GetKey("x"))
        {
            rb.velocity = new Vector3(0,0,0);
        }

        rb.AddForce(movement * speed);

        

    }

    private void launchPlayer()
    {
        if (power >= 2.0f)
        {
            Vector3 jump = new Vector3(0.0f, upSpeed, 0.0f);
            power -= 2.0f;
            rb.AddForce(jump * speed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.gameObject.CompareTag("Ground"))
        {
            power += 2.0f;
        }
    }

    private void OnCollisionStay(Collision collision)
    {

        if ((Time.time - currentTime) > 0.2f  && power < 10.0f)
        {
            power += 0.5f;
            currentTime = Time.time;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
            other.gameObject.SetActive(false);
        }
    }
}
