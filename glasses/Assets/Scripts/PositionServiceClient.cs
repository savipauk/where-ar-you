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
    public double latitude { get; set; }
    public double longitude { get; set; }
    public double altitude { get; set; }
    public double timestamp { get; set; }
}

public class PositionServiceClient : MonoBehaviour
{
    public string PositionServerAddress = "http://localhost:5000";
    public float PositionServerFetchInterval = 0.5f;

    public GameObject MarkerPoolControllerObject;

    public float DesiredLocalAccuracyInMeters = 1f;
    public float UpdateLocalDistanceInMeters = 1f;

    private GeoPosition origin = new GeoPosition { latitude = 45.80944313945194, longitude = 15.999005239514487, altitude = 0.5 };
    private float fetchTimer = 0f;

    private bool waitingForGeolocation = true;

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
            
            // Keep only the position with the latest (largest) timestamp for each user
            List<GeoPosition> latest = new List<GeoPosition>();
            Dictionary<string, GeoPosition> userPositions = new Dictionary<string, GeoPosition>();
            foreach (GeoPosition pos in deserialized)
            {
                if (!userPositions.ContainsKey(pos.username) || pos.timestamp > userPositions[pos.username].timestamp)
                {
                    userPositions[pos.username] = pos;
                }
            }
            latest.AddRange(userPositions.Values);

            if (deserialized.Count == 0)
            {
                yield break;
            }

            MarkerPoolControllerObject.GetComponent<MarkerPoolController>().SetMarkers(latest, origin);
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
        if (waitingForGeolocation && Input.location.status == LocationServiceStatus.Running)
        {
            waitingForGeolocation = false;

            LocationInfo locInfo = Input.location.lastData;
            origin = new GeoPosition
            {
                latitude = locInfo.latitude,
                longitude = locInfo.longitude,
                altitude = locInfo.altitude
            };

            Input.location.Stop();
        }
    }
}
