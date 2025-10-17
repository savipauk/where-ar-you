import { useEffect } from 'react';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { View, ActivityIndicator } from 'react-native';
import { ThemedText } from '@/components/themed-text';

export default function OAuth2Redirect() {
  const { code } = useLocalSearchParams<{ code?: string }>();
  const router = useRouter();

  useEffect(() => {
    if (!code) {
      return;
    }

    const fetchTokens = async () => {
      const tokenResponse = await fetch('https://oauth2.googleapis.com/token', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({
          code,
          client_id: process.env.EXPO_PUBLIC_GOOGLE_CLIENT_ID || '',
          client_secret: process.env.EXPO_PUBLIC_GOOGLE_SECRET || '',
          redirect_uri: 'YOUR_REDIRECT_URI',
          grant_type: 'authorization_code'
        })
      });

      const data = await tokenResponse.json();
      console.log(data);
    }

    console.log('OAuth code:', code);



    // router.replace('/');
  }, [code, router]);

  return (
    <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', gap: 16 }}>
      <ActivityIndicator />
      <ThemedText>Signing in...</ThemedText>
    </View>
  );
}

