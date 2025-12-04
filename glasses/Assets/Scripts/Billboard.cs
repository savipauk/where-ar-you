using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool AllowScaleWithDistance = false;

    public float DynamicScaleFactor = 0.01f;
    public float StaticScaleFactor = 0.1f;

    public Vector3 RotationOffset = new Vector3(90, 0, 0);

    void Update()
    {
        // Turns the object to face the camera and scales it as needed

        Vector3 direction = Camera.main.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(-direction);
        transform.rotation = lookRotation;

        // Rotate 90 degrees around the x-axis; this is because the plane model would otherwise 
        // face the camera edge-on.
        transform.Rotate(RotationOffset);

        if (!AllowScaleWithDistance)
        {
            // Maintain a constant scale regardless of distance to camera
            float distanceToCamera = direction.magnitude;
            transform.localScale = Vector3.one * distanceToCamera * DynamicScaleFactor;
        }
        else
        {
            transform.localScale = Vector3.one * StaticScaleFactor;
        }
    }
}
