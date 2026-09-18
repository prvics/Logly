import axios, { AxiosError } from "axios";
import { Platform } from "react-native";
import * as SecureStore from "expo-secure-store";
import type { ApiErrorResponse } from "../types/api";

const DEV_MACHINE_LAN_IP = process.env.EXPO_PUBLIC_API_LAN_IP ?? "localhost";

function resolveBaseUrl(): string {
  if (Platform.OS === "web") {
    return "http://localhost:5000";
  }

  if (Platform.OS === "android") {
    // android mobile
    //return `http://${DEV_MACHINE_LAN_IP}:5000`
    return "http://10.0.2.2:5000";
  }

  // iOS mobile
  return `http://${DEV_MACHINE_LAN_IP}:5000`;
}

export const API_BASE_URL = resolveBaseUrl();

const AUTH_TOKEN_KEY = "logly_auth_token";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10_000,
  headers: {
    "Content-Type": "application/json",
  },
});

// JWT if saved
apiClient.interceptors.request.use(async (config) => {
  const token = await SecureStore.getItemAsync(AUTH_TOKEN_KEY);

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export class ApiError extends Error {
  public readonly status: number | null;
  public readonly fieldErrors: Record<string, string[]>;

  constructor(
    message: string,
    status: number | null,
    fieldErrors: Record<string, string[]> = {}
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.fieldErrors = fieldErrors;
  }

  fieldError(fieldName: string): string | undefined {
    return this.fieldErrors[fieldName]?.[0];
  }
}

// Válasz interceptor: a backend KÉTFÉLE hiba-alakot ad vissza:
// 1) FluentValidation hiba: { message: "Érvénytelen kérés.", errors: {...} }
//    -> ValidationExtensions.ToBadRequest()
// 2) Egyedi hiba: { message: "..." } (pl. Unauthorized, NotFound eseteknél)
//    -> pl. NotFound(new { message = "..." })
// Mindkettőben van "message" mező, az "errors" csak az elsőben.
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    const data = error.response?.data;
    const status = error.response?.status ?? null;

    const message =
      data?.message ?? error.message ?? "Ismeretlen hiba történt.";

    const fieldErrors = data?.errors ?? {};

    return Promise.reject(new ApiError(message, status, fieldErrors));
  }
);

export { AUTH_TOKEN_KEY };
