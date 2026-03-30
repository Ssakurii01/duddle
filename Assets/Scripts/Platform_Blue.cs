using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform_Blue : MonoBehaviour
{

    // For moving platforms
    private bool To_Right = true;
    private float Offset = 1.2f;

    public float Movement_Speed = 3f;

    void FixedUpdate()
    {
        // Move the platform — speed increases with height for more adrenaline
        Vector3 Top_Left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        float heightBoost = 1f + Mathf.Clamp(transform.position.y / 100f, 0f, 2f);
        float step = Movement_Speed * heightBoost * Time.fixedDeltaTime;

        if (To_Right) // Move to right
        {
            if (transform.position.x < -Top_Left.x - Offset)
                transform.position += new Vector3(step, 0, 0);
            else
                To_Right = false;
        }
        else // Move to left
        {
            if (transform.position.x > Top_Left.x + Offset)
                transform.position -= new Vector3(step, 0, 0);
            else
                To_Right = true;
        }
    }
}
