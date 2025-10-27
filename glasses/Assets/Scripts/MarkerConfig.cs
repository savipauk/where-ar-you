using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class MarkerConfig : MonoBehaviour
{
    public GameObject markerPrefab;

    private string markerType = "2d_waypoint";
    private bool displayDistanceIndicator = false;
    private bool scaleWithDistance = false;

    void Start()
    {
        On2DWaypointClicked(); // Start with 2D Waypoint selected
    }

    void RefreshMarkers()
    {
        var existingMarkers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (var marker in existingMarkers)
        {
            marker.GetComponent<LocationMarker>().UpdateMarkerStyle(markerType, displayDistanceIndicator, scaleWithDistance);
        }
    }

    public void On2DCircleClicked()
    {
        markerType = "2d_circle";
        RefreshMarkers();
    }

    public void On2DSquareClicked()
    {
        markerType = "2d_square";
        RefreshMarkers();
    }

    public void On2DWaypointClicked()
    {
        markerType = "2d_waypoint";
        RefreshMarkers();
    }

    public void On3DSphereClicked()
    {
        markerType = "3d_sphere";
        RefreshMarkers();
    }

    public void On3DCubeClicked()
    {
        markerType = "3d_cube";
        RefreshMarkers();
    }

    public void On3DArrowClicked()
    {
        markerType = "3d_arrow";
        RefreshMarkers();
    }
}
