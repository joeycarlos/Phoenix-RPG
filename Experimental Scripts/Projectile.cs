using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {


    [SerializeField] float projectileLifeTime = 3f;

    private Rigidbody rb;

    // Use this for initialization
    void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
        Destroy(gameObject, projectileLifeTime);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

}
