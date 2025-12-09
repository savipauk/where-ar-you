using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class GeoPosition
{
    public string username { get; set; }
    public float latitude { get; set; }
    public float longitude { get; set; }
    public int timestamp { get; set; }
}

public class PositionServiceClient : MonoBehaviour
{
    public string PositionServerAddress = "http://localhost:5000";
    public float PositionServerFetchInterval = 0.5f;

    public float DesiredLocalAccuracyInMeters = 1f;
    public float UpdateLocalDistanceInMeters = 1f;

    private GeoPosition origin = null;
    private float fetchTimer = 0f;

    IEnumerator FetchPositions()
    {
        UnityWebRequest request = UnityWebRequest.Get(PositionServerAddress);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error fetching positions: {request.error}");
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;

            List<GeoPosition> deserialized = JsonConvert.DeserializeObject<List<GeoPosition>>(jsonResponse);

            if (origin == null || deserialized.Count == 0)
            {
                yield break;
            }

            GetComponent<MarkerPoolController>().SetMarkers(deserialized, origin);
        }
    }

    private void Start()
    {
        Input.location.Start(DesiredLocalAccuracyInMeters, UpdateLocalDistanceInMeters);
    }

    private void Update()
    {
        // Gets positions from the server at a regular interval

        fetchTimer -= Time.deltaTime;
        if (fetchTimer <= 0f)
        {
            StartCoroutine(FetchPositions());
            fetchTimer = PositionServerFetchInterval;
        }

        // Get the origin (user position) only initially - rely on device tracking afterwards
        if (origin == null && Input.location.status == LocationServiceStatus.Running)
        {
            LocationInfo locInfo = Input.location.lastData;
            origin = new GeoPosition
            {
                latitude = locInfo.latitude,
                longitude = locInfo.longitude,
               //altitude = locInfo.altitude
            };
        }
    }
}
