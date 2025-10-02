import ParallaxScrollView from "@/app-example/components/parallax-scroll-view";
import { ThemedText } from "@/components/themed-text";
import { Button, StyleSheet } from 'react-native';
import { Image } from 'expo-image';
import { ThemedView } from "@/components/themed-view";
import { HelloWave } from "@/components/hello-wave";
import { useState } from "react";
import * as Location from "expo-location";

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

    setLocation({ lat: loc.coords.latitude, lon: loc.coords.longitude, timestamp: loc.timestamp });
    console.log("Got location:", loc.coords);
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

