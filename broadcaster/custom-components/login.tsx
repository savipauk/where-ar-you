import React from 'react';
import { View, Alert, StyleSheet } from 'react-native';
import {
  GoogleSignin,
  GoogleSigninButton,
  statusCodes,
  isErrorWithCode
} from '@react-native-google-signin/google-signin';
import * as SecureStore from 'expo-secure-store';

GoogleSignin.configure({
  webClientId: process.env.EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID,
  offlineAccess: true,
  scopes: ['profile', 'email'],
});

type Props = {
  onLogin?: () => void;
};

export default function Login({ onLogin }: Props) {
  const googleSignIn = async () => {
    try {
      await GoogleSignin.hasPlayServices();

      const response = await GoogleSignin.signIn();

      const token = response.data?.idToken || response.data?.idToken;
      const user = response.data?.user || response.data?.user || null;

      if (!token) {
        Alert.alert('Error', 'No ID Token found in response');
        return;
      }

      console.log("Success! ID Token:", token);

      if (user) {
        await SecureStore.setItemAsync('user_info', JSON.stringify(user));
      }

      await SecureStore.setItemAsync('id_token', token);

      onLogin?.();
    } catch (error) {
      if (isErrorWithCode(error)) {
        switch (error.code) {
          case statusCodes.SIGN_IN_CANCELLED:
            Alert.alert("Cancelled", "User cancelled the login flow");
            break;
          case statusCodes.IN_PROGRESS:
            Alert.alert("In Progress", "Sign in is already in progress");
            break;
          case statusCodes.PLAY_SERVICES_NOT_AVAILABLE:
            Alert.alert("Error", "Google Play Services not available or outdated");
            break;
          default:
            Alert.alert("Error", error.message);
            console.error("Custom error:", error);
        }
      } else {
        console.error(error);
      }
    }
  };

  return (
    <View style={styles.container}>
      <GoogleSigninButton
        size={GoogleSigninButton.Size.Wide}
        color={GoogleSigninButton.Color.Dark}
        onPress={googleSignIn}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    alignItems: 'center',
    justifyContent: 'center',
    padding: 10,
  },
});
