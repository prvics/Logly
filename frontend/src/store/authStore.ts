import { create } from "zustand";
import * as SecureStore from "expo-secure-store";
import { AUTH_TOKEN_KEY } from "../api/client";
import type { AuthResponse } from "../types/api";

const AUTH_USER_KEY = "logly_auth_user";


export interface StoredAuthUser {
  userId: string;
  email: string;
  displayName: string | null;
}

interface AuthState {
  token: string | null;
  user: StoredAuthUser | null;
  expiresAtUtc: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  loginWithResponse: (response: AuthResponse) => Promise<void>;
  logout: () => Promise<void>;
  restoreSession: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  user: null,
  expiresAtUtc: null,
  isAuthenticated: false,
  isLoading: true,

  loginWithResponse: async (response: AuthResponse) => {
    const user: StoredAuthUser = {
      userId: response.userId,
      email: response.email,
      displayName: response.displayName,
    };

    await SecureStore.setItemAsync(AUTH_TOKEN_KEY, response.accessToken);
    await SecureStore.setItemAsync(AUTH_USER_KEY, JSON.stringify(user));

    set({
      token: response.accessToken,
      user,
      expiresAtUtc: response.expiresAtUtc,
      isAuthenticated: true,
      isLoading: false,
    });
  },

  logout: async () => {
    await SecureStore.deleteItemAsync(AUTH_TOKEN_KEY);
    await SecureStore.deleteItemAsync(AUTH_USER_KEY);

    set({
      token: null,
      user: null,
      expiresAtUtc: null,
      isAuthenticated: false,
      isLoading: false,
    });
  },

  restoreSession: async () => {
    try {
      const token = await SecureStore.getItemAsync(AUTH_TOKEN_KEY);
      const userJson = await SecureStore.getItemAsync(AUTH_USER_KEY);

      if (token && userJson) {
        const user: StoredAuthUser = JSON.parse(userJson);

        set({
          token,
          user,
          isAuthenticated: true,
          isLoading: false,
        });
      } else {
        set({ isLoading: false });
      }
    } catch {
      set({ isLoading: false });
    }
  },
}));