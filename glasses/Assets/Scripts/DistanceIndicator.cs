using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceIndicator : MonoBehaviour
{
    // Difference in height from the marker location to the point the marker is pointing at actually
    public float DistanceOffsetHeight = 2.0f;

    // Update text value continually based on the distance to the marker point
    void Update()
    {

        var dist = Vector3.Distance(Camera.main.transform.position, transform.position - new Vector3(0, DistanceOffsetHeight, 0));

        GetComponent<TMPro.TextMeshPro>().text = $"{dist:F1} m";
    }
}
