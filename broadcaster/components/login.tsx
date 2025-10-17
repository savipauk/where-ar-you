import React, { useEffect } from 'react';
import * as WebBrowser from 'expo-web-browser';
import * as Google from 'expo-auth-session/providers/google';
import { Button } from 'react-native';

WebBrowser.maybeCompleteAuthSession();

type GoogleLoginButtonProps = {
  onLogin: (id: string) => Promise<void>;
}

export default function GoogleLoginButton({ onLogin }: GoogleLoginButtonProps) {
  const [request, response, promptAsync] = Google.useAuthRequest({
    clientId: process.env.EXPO_PUBLIC_GOOGLE_CLIENT_ID || '',
    scopes: ['openid', 'profile', 'email'],
    redirectUri: process.env.EXPO_PUBLIC_REDIRECT_URI || '',
  });

  useEffect(() => {
    if (response?.type === 'success') {
      const { id_token } = response.params;
      onLogin(id_token);
    }
  }, [onLogin, response]);

  return <Button title="Sign in with Google" onPress={() => promptAsync()} />;
}

