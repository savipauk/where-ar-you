using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        // Turns the object to face the camera

        Vector3 direction = Camera.main.transform.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;

        // Rotate 90 degrees around the x-axis; this is because the plane model would otherwise 
        // face the camera edge-on.
        transform.Rotate(new Vector3(90, 0, 0));
    }
}
