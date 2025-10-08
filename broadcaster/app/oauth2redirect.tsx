import { useEffect } from 'react';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { View, ActivityIndicator } from 'react-native';
import { ThemedText } from '@/components/themed-text';

export default function OAuth2Redirect() {
  const { code } = useLocalSearchParams<{ code?: string }>();
  const router = useRouter();

  useEffect(() => {
    if (code) {
      console.log('OAuth code:', code);
      // TODO: Exchange code for access token here after implementing it on the backend

      // router.replace('/');
    }
  }, [code, router]);

  return (
    <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center', gap: 16 }}>
      <ActivityIndicator />
      <ThemedText>Signing in...</ThemedText>
    </View>
  );
}

