using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_Brown : MonoBehaviour {

    private bool Fall_Down = false;

	void FixedUpdate () 
    {
        if (Fall_Down)
            transform.position -= new Vector3(0, 0.15f, 0);
	}

    public void Deactive()
    {
        StartCoroutine(DelayedFall());
    }

    IEnumerator DelayedFall()
    {
        // Wait a short moment so the player's jump force applies before the platform falls
        yield return new WaitForSeconds(0.15f);

        GetComponent<EdgeCollider2D>().enabled = false;
        GetComponent<PlatformEffector2D>().enabled = false;

        Fall_Down = true;
    }
}
