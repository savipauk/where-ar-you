using System.Runtime.CompilerServices;
using UnityEngine;

public class LocationMarker : MonoBehaviour
{
    public float markerOffset = 4.25f;

    public GameObject billboardPrefab = null;
    public Material WaypointMat = null;
    public Material CircleMat;
    public Material SquareMat;

    public GameObject ArrowPrefab = null;
    public GameObject SpherePrefab = null;
    public GameObject CubePrefab = null;

    public GameObject DistanceIndicatorPrefab = null;

    public void UpdateMarkerStyle(string markerType, bool displayDistanceIndicator, bool scaleWithDistance)
    {
        RemoveCurrentMarkerStyle();
        CreateMarkerStyle(markerType, displayDistanceIndicator, scaleWithDistance);
    }

    private void RemoveCurrentMarkerStyle()
    {
        if (transform.childCount < 3)
        {
            // No marker style to remove
            return;
        }

        // Remove the last added child
        Destroy(transform.GetChild(transform.childCount - 1).gameObject);
    }

    private void CreateMarkerStyle(string markerType, bool displayDistanceIndicator, bool scaleWithDistance)
    {
        if (markerType == "2d_waypoint" || markerType == "2d_circle" || markerType == "2d_square")
        {
            // Create a billboard object and set the matching material
            // according to the markerType, as well as other options
            // (should the billboard scale with distance?)

            var billboard = Instantiate(billboardPrefab, transform.position + new Vector3(0, markerOffset, 0), Quaternion.identity);
            billboard.transform.SetParent(transform);

            if (displayDistanceIndicator)
            {
                var distanceIndicator = Instantiate(DistanceIndicatorPrefab, Vector3.zero, Quaternion.identity);

                distanceIndicator.AddComponent<Billboard>();
                distanceIndicator.GetComponent<Billboard>().RotationOffset = Vector3.zero;
                distanceIndicator.GetComponent<Billboard>().AllowScaleWithDistance = true;
                distanceIndicator.GetComponent<Billboard>().StaticScaleFactor = 0.5f;
                
                distanceIndicator.transform.SetParent(billboard.transform);
                distanceIndicator.transform.localPosition = new Vector3(0, 0, -6.0f);
            }

            billboard.GetComponent<Renderer>().material = markerType switch
            {
                "2d_waypoint" => WaypointMat,
                "2d_circle" => CircleMat,
                "2d_square" => SquareMat,
                _ => null,
            };

            billboard.gameObject.GetComponent<Billboard>().AllowScaleWithDistance = scaleWithDistance;
        }
        else
        {
            // We're not using a billboard, so just instantiate a matching 3d prefab according to the markerType

            var prefab = markerType switch
            {
                "3d_arrow" => ArrowPrefab,
                "3d_sphere" => SpherePrefab,
                "3d_cube" => CubePrefab,
                _ => null,
            };

            var obj = Instantiate(prefab, transform.position + new Vector3(0, markerOffset, 0), Quaternion.identity);

            if (displayDistanceIndicator)
            {
                var distanceIndicator = Instantiate(DistanceIndicatorPrefab, transform.position + new Vector3(0, markerOffset + 1.0f, 0), Quaternion.identity);
                distanceIndicator.AddComponent<Billboard>();
                distanceIndicator.GetComponent<Billboard>().RotationOffset = Vector3.zero;
                distanceIndicator.GetComponent<Billboard>().StaticScaleFactor = 0.01f;
                distanceIndicator.transform.SetParent(obj.transform);
            }

            obj.transform.SetParent(transform);
        }
    }
}
