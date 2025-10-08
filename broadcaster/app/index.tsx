import ParallaxScrollView from "@/app-example/components/parallax-scroll-view";
import { ThemedText } from "@/components/themed-text";
import { Button, StyleSheet } from 'react-native';
import { Image } from 'expo-image';
import { ThemedView } from "@/components/themed-view";
import { HelloWave } from "@/components/hello-wave";
import { useState } from "react";
import * as Location from "expo-location";
import GoogleLoginButton from "@/components/login";

export default function Index() {
  const [location, setLocation] = useState<{
    lat: number;
    lon: number;
    timestamp: number,
  } | null>(null);

  const updateLocation = async () => {
    const { status } = await Location.requestForegroundPermissionsAsync();
    if (status !== "granted") {
      console.warn("Location permission denied!");
      return;
    }

    const loc = await Location.getCurrentPositionAsync({
      accuracy: Location.Accuracy.BestForNavigation
    });
    console.log("Got location:", loc.coords);

    setLocation({ lat: loc.coords.latitude, lon: loc.coords.longitude, timestamp: loc.timestamp });

    const server_url = process.env.EXPO_PUBLIC_RELAY_SERVER_URL || "";
    console.log("SERVER_URL: " + server_url)

    const body = {
      username: "placeholder username",
      latitude: loc.coords.latitude,
      longitude: loc.coords.longitude,
      timestamp: loc.timestamp
    }

    console.log(body)

    const params = {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body)
    }

    try {
      const res = await fetch(server_url + "/location", params);
      const data = await res.json();
      console.log(data);
    } catch (e) {
      console.error(e)
    }
  }

  return (
    <ParallaxScrollView
      headerBackgroundColor={{ light: '#A1CEDC', dark: '#1D3D47' }}
      headerImage={
        <Image
          source={require('@/assets/images/partial-react-logo.png')}
          style={styles.reactLogo}
        />
      }>

      <ThemedView style={styles.titleContainer}>
        <ThemedText type="title">broadcaster!</ThemedText>
        <HelloWave />
      </ThemedView>

      <ThemedText style={{ marginTop: 20 }}>
        Location: {location ? `${location.lat}, ${location.lon}` : "Unknown"}
      </ThemedText>

      <GoogleLoginButton onLogin={function(id: string): void {
        throw new Error("Function not implemented.");
      }} />

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

