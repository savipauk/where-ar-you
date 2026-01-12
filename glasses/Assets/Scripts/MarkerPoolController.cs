using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class MarkerPoolController : MonoBehaviour
{
    public GameObject markerPrefab;
    public GameObject markerConfig;

    public void ClearMarkers()
    {
        var markers = GameObject.FindGameObjectsWithTag("Marker");
        foreach (GameObject marker in markers)
        {
            Destroy(marker);
        }
    }

    Vector3 GetUnityPosFromGeoPosition(GeoPosition geoPos, GeoPosition origin)
    {
        // Note: This method only works for small dx/dz distances
        const double M_R_EARTH = 6378000;
        double M_PER_DEG_LONGITUDE = (3.1415 / 180) * M_R_EARTH * Mathf.Cos((float)(origin.longitude * 3.1415 / 180));
        double M_PER_DEG_LATITUDE = (3.1415 / 180) * M_R_EARTH * Mathf.Cos((float)(origin.latitude * 3.1415 / 180));

        // Assume origin is on (0, 0, 0) -> convert geoPos to local Unity coordinates
        double dx = geoPos.longitude - origin.longitude;
        double dy = geoPos.altitude - origin.altitude;
        double dz = geoPos.latitude - origin.latitude;

        return new Vector3(
            (float)(dx * M_PER_DEG_LONGITUDE),
            (float)(dy),
            (float)(dz * M_PER_DEG_LATITUDE)
        );
    }

    public void SetMarkers(List<GeoPosition> positions, GeoPosition origin)
    {
        ClearMarkers();
        foreach (GeoPosition pos in positions)
        {
            Instantiate(markerPrefab, GetUnityPosFromGeoPosition(pos, origin), Quaternion.identity);
        }
        markerConfig.GetComponent<MarkerConfig>().RefreshMarkerDecoration();
    }
}
