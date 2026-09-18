import { create } from "zustand";
import { authStorage } from "../storage/authStorage";
import type { AuthResponse, AuthUser } from "../api/types";

type AuthState = {
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  user: AuthUser | null;

  loginWithResponse: (response: AuthResponse) => Promise<void>;
  logout: () => Promise<void>;
  restoreSession: () => Promise<void>;
};

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  isAuthenticated: false,
  isLoading: true,
  user: null,

  loginWithResponse: async (response) => {
    console.log("AUTH RESPONSE KEYS", Object.keys(response));
    console.log("AUTH RESPONSE USER", response.user);

    try {
      await Promise.all([
        authStorage.setAccessToken(response.accessToken),
        authStorage.setUser(response.user),
      ]);

      set({
        accessToken: response.accessToken,
        isAuthenticated: true,
        isLoading: false,
        user: response.user,
      });
    } catch (error) {
      console.error("AUTH STORAGE SAVE FAILED", error);

      try {
        await authStorage.clear();
      } catch {
        // The in-memory auth state is still cleared below.
      }

      set({
        accessToken: null,
        isAuthenticated: false,
        isLoading: false,
        user: null,
      });

      throw error;
    }
  },

  logout: async () => {
    try {
      await authStorage.clear();
    } finally {
      set({
        accessToken: null,
        isAuthenticated: false,
        isLoading: false,
        user: null,
      });
    }
  },

  restoreSession: async () => {
    try {
      const [accessToken, user] = await Promise.all([
        authStorage.getAccessToken(),
        authStorage.getUser<AuthUser>(),
      ]);

      console.log("RESTORE SESSION", {
        hasAccessToken: Boolean(accessToken),
        hasUser: Boolean(user),
      });

      if (!accessToken || !user) {
        set({
          accessToken: null,
          isAuthenticated: false,
          isLoading: false,
          user: null,
        });

        return;
      }

      set({
        accessToken,
        isAuthenticated: true,
        isLoading: false,
        user,
      });
    } catch (error) {
      console.error("RESTORE SESSION FAILED", error);

      set({
        accessToken: null,
        isAuthenticated: false,
        isLoading: false,
        user: null,
      });
    }
  },
}));