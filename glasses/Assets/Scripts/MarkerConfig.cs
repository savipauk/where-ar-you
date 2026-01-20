using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;

public class MarkerConfig : MonoBehaviour
{
    public GameObject markerPrefab;
    public GameObject gpsCoordsDisplay;
    public GameObject markerPoolController;

    private string markerType = "2d_waypoint";
    private bool displayDistanceIndicator = false;
    private bool scaleWithDistance = false;
    private bool useGPSAltitude = true;

    void Start()
    {
        On2DWaypointClicked(); // Start with 2D Waypoint selected
    }
    void Update()
    {
        gpsCoordsDisplay.GetComponent<TMP_Text>().text = $"Lat {Input.location.lastData.latitude:F6}, Lon {Input.location.lastData.longitude:F6}, Alt {Input.location.lastData.altitude:F2}m";
    }

    public void RefreshMarkerDecoration()
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
        RefreshMarkerDecoration();
    }

    public void On2DSquareClicked()
    {
        markerType = "2d_square";
        RefreshMarkerDecoration();
    }

    public void On2DWaypointClicked()
    {
        markerType = "2d_waypoint";
        RefreshMarkerDecoration();
    }

    public void On3DSphereClicked()
    {
        markerType = "3d_sphere";
        RefreshMarkerDecoration();
    }

    public void On3DCubeClicked()
    {
        markerType = "3d_cube";
        RefreshMarkerDecoration();
    }

    public void On3DArrowClicked()
    {
        markerType = "3d_arrow";
        RefreshMarkerDecoration();
    }
    public void OnToggleDistanceIndicator()
    {
        displayDistanceIndicator = !displayDistanceIndicator;
        RefreshMarkerDecoration();
    }

    public void OnToggleScaleWithDistance()
    {
        scaleWithDistance = !scaleWithDistance;
        RefreshMarkerDecoration();
    }

    public void OnToggleUseGPSAltitude()
    {
        useGPSAltitude = !useGPSAltitude;
        markerPoolController.GetComponent<MarkerPoolController>().SetUseGPSAltitude(useGPSAltitude);
    }
}
