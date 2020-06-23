using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fragment : MonoBehaviour {
    float startTime;
    float time;

    void Start() {
        this.startTime = Random.Range(5f, 10f);
        this.time = this.startTime;
    }

    void Update() {
    	this.time -= Time.deltaTime;
        if(this.time <= 0f)
        	Destroy(gameObject);
        // else
        // 	GetComponent<MeshRenderer>().material.color = new Color(255, 255, 255, alpha);
    }
}
