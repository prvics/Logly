import axios, { AxiosError } from "axios";
import { authStorage } from "../storage/authStorage";

const lanIp = process.env.EXPO_PUBLIC_API_LAN_IP;

if (!lanIp) {
  throw new Error(
    "Hiányzik az EXPO_PUBLIC_API_LAN_IP változó a frontend/.env fájlból."
  );
}

const baseURL = lanIp.replace(/\/$/, "");

export type ValidationError = {
  field: string;
  message: string;
};

type ApiErrorResponse = {
  message?: string;
  errors?: ValidationError[];
  title?: string;
};

export class ApiError extends Error {
  readonly status: number | null;
  readonly errors: ValidationError[];

  constructor(
    message: string,
    status: number | null = null,
    errors: ValidationError[] = []
  ) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }

  fieldError(field: string): string | null {
    const normalizedField = field.toLowerCase();

    return (
      this.errors.find(
        (error) => error.field.toLowerCase() === normalizedField
      )?.message ?? null
    );
  }
}

export const apiClient = axios.create({
  baseURL,
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
  },
  timeout: 15_000,
});

apiClient.interceptors.request.use(async (config) => {
  const token = await authStorage.getAccessToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiErrorResponse>) => {
    if (!error.response) {
      return Promise.reject(
        new ApiError(
          "Nem sikerült kapcsolódni a szerverhez. Ellenőrizd, hogy a backend fut-e, és az API címe helyes-e."
        )
      );
    }

    const { status, data } = error.response;

    const message =
      data?.message ??
      data?.title ??
      "A szerver nem tudta feldolgozni a kérést.";

    return Promise.reject(
      new ApiError(message, status, data?.errors ?? [])
    );
  }
);