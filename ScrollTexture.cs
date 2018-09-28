using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollTexture : MonoBehaviour {


    public float scrollSpeed1 = 0.90f;
    public float scrollSpeed2 = 0.90f;
    // Use this for initialization
    Material scrollMaterial;

    private void Start()
    {
        scrollMaterial = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void FixedUpdate () {
        float offset1 = Time.time * scrollSpeed1;
        float offset2 = Time.time * scrollSpeed2;
        scrollMaterial.mainTextureOffset = new Vector2(-offset2, offset1);
	}
}
