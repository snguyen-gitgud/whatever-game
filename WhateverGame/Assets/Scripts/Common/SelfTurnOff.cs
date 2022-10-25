using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfTurnOff : MonoBehaviour {

    public float dur = 5.0f;

	// Use this for initialization
	void Start () {
        StartCoroutine(selfTurnOff());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator selfTurnOff()
    {
        yield return new WaitForSeconds(dur);
        Destroy(this.gameObject);
    }
}
