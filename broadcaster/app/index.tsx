import ParallaxScrollView from "@/components/parallax-scroll-view";
import { ThemedText } from "@/components/themed-text";
import { Alert, Button, StyleSheet } from 'react-native';
import { Image } from 'expo-image';
import { ThemedView } from "@/components/themed-view";
import { HelloWave } from "@/components/hello-wave";
import { useState } from "react";
import * as Location from "expo-location";
import { getItemAsync } from "expo-secure-store";
import Login from "@/custom-components/login";

export default function Index() {
  const [location, setLocation] = useState<{
    lat: number;
    lon: number;
    altitude: number;
    timestamp: number,
  } | null>(null);

  const updateLocation = async () => {
    const token = await getItemAsync('id_token');
    const user = await getItemAsync('user');

    if (!token) {
      Alert.alert("Authentication Required", "Please sign in with Google first.");
      console.log("No OAuth token!");
      return;
    }

    const { status } = await Location.requestForegroundPermissionsAsync();
    if (status !== "granted") {
      Alert.alert("Permission Denied", "Allow location access to sync data.");
      console.warn("Location permission denied!");
      return;
    }

    const loc = await Location.getCurrentPositionAsync({
      accuracy: Location.Accuracy.BestForNavigation
    });
    console.log("Got location:", loc.coords);

    setLocation({
      lat: loc.coords.latitude,
      lon: loc.coords.longitude,
      altitude: loc.coords.altitudeAccuracy || 0,
      timestamp: loc.timestamp
    });

    const server_url = process.env.EXPO_PUBLIC_RELAY_SERVER_URL || "";
    console.log("SERVER_URL: " + server_url)

    let userEmail = "unknown";
    if (user) {
      try {
        const userData = JSON.parse(user);
        userEmail = userData.email || "unknown";
      } catch (e) {
        console.error("Failed to parse user_info", e);
      }
    }

    const body = {
      username: userEmail,
      latitude: loc.coords.latitude,
      longitude: loc.coords.longitude,
      altitude: loc.coords.altitudeAccuracy || 0,
      timestamp: loc.timestamp
    }

    console.log(body)

    const params = {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`,
      },
      body: JSON.stringify(body)
    };
    Alert.alert("server url", server_url);

    try {
      const res = await fetch(server_url + "/location", params);

      if (res.ok) {
        const data = await res.json();
        console.log(data);
        Alert.alert("Success", "Location synced to backend!");
      } else {
        const text = await res.text();
        console.error("Server Error:", res.status, text);
        Alert.alert("Server Error", `Status: ${res.status}`);
      }
    } catch (e) {
      console.error(e)
      Alert.alert("Network Error", "Could not reach the server.");
    }
  }

  return (
    <ParallaxScrollView
      headerBackgroundColor={{ light: '#A1CEDC', dark: '#1D3D47' }}
      headerImage={
        <Image
          source={require('../assets/images/partial-react-logo.png')}
          style={styles.reactLogo}
        />
      }>

      <ThemedView style={styles.titleContainer}>
        <ThemedText type="title">broadcaster!</ThemedText>
        <HelloWave />
      </ThemedView>

      <ThemedText style={{ marginTop: 20 }}>
        Location: {location ? `${location.lat}, ${location.lon}, ${location.altitude}` : "Unknown"}
      </ThemedText>

      <Login onLogin={() => Alert.alert("Logged In", "Token saved securely.")} />

      <Button title="Update Location" onPress={updateLocation} />

    </ParallaxScrollView>
  );
}

const styles = StyleSheet.create({
  titleContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  stepContainer: {
    gap: 8,
    marginBottom: 8,
  },
  reactLogo: {
    height: 178,
    width: 290,
    bottom: 0,
    left: 0,
    position: 'absolute',
  },
});

