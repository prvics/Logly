import { Platform } from "react-native";
import * as SecureStore from "expo-secure-store";

const ACCESS_TOKEN_KEY = "logly_access_token";
const USER_KEY = "logly_user";

async function setItem(key: string, value: string): Promise<void> {
  if (Platform.OS === "web") {
    globalThis.localStorage.setItem(key, value);
    return;
  }

  await SecureStore.setItemAsync(key, value);
}

async function getItem(key: string): Promise<string | null> {
  if (Platform.OS === "web") {
    return globalThis.localStorage.getItem(key);
  }

  return SecureStore.getItemAsync(key);
}

async function removeItem(key: string): Promise<void> {
  if (Platform.OS === "web") {
    globalThis.localStorage.removeItem(key);
    return;
  }

  await SecureStore.deleteItemAsync(key);
}

export const authStorage = {
  getAccessToken: (): Promise<string | null> => {
    return getItem(ACCESS_TOKEN_KEY);
  },

  setAccessToken: (accessToken: string): Promise<void> => {
    return setItem(ACCESS_TOKEN_KEY, accessToken);
  },

  removeAccessToken: (): Promise<void> => {
    return removeItem(ACCESS_TOKEN_KEY);
  },

  getUser: async <T>(): Promise<T | null> => {
    const userJson = await getItem(USER_KEY);

    if (!userJson) {
      return null;
    }

    try {
      return JSON.parse(userJson) as T;
    } catch {
      await removeItem(USER_KEY);
      return null;
    }
  },

  setUser: <T>(user: T): Promise<void> => {
    return setItem(USER_KEY, JSON.stringify(user));
  },

  removeUser: (): Promise<void> => {
    return removeItem(USER_KEY);
  },

  clear: async (): Promise<void> => {
    await Promise.all([
      removeItem(ACCESS_TOKEN_KEY),
      removeItem(USER_KEY),
    ]);
  },
};