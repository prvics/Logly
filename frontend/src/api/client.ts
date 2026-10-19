import axios from "axios";
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
  errors?: Record<string, string[]> | ValidationError[];
  title?: string;
};

function normalizeValidationErrors(value: unknown): ValidationError[] {
  if (Array.isArray(value)) {
    return value.filter(
      (item): item is ValidationError =>
        typeof item === "object" &&
        item !== null &&
        typeof item.field === "string" &&
        typeof item.message === "string"
    );
  }

  if (typeof value !== "object" || value === null) {
    return [];
  }

  return Object.entries(value).flatMap(([field, messages]) => {
    if (!Array.isArray(messages)) {
      return [];
    }

    return messages
      .filter((message): message is string => typeof message === "string")
      .map((message) => ({
        field,
        message,
      }));
  });
}

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
      this.errors.find((error) => {
        const errorField = error.field
          .split(".")
          .pop()
          ?.toLowerCase();

        return errorField === normalizedField;
      })?.message ?? null
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

  if (token && !config.headers.has("Authorization")) {
    config.headers.set("Authorization", `Bearer ${token}`);
  }

  return config;
});

let logoutInProgress: Promise<void> | null = null;

async function clearExpiredSession(): Promise<void> {
  if (!logoutInProgress) {
    logoutInProgress = (async () => {
      const { useAuthStore } = await import("../store/authStore");

      try {
        await useAuthStore.getState().logout();
      } finally {
        useAuthStore.setState({
          accessToken: null,
          user: null,
          isAuthenticated: false,
          isLoading: false,
        });
      }
    })().finally(() => {
      logoutInProgress = null;
    });
  }

  await logoutInProgress;
}

function isLoginOrRegisterRequest(url: string | undefined): boolean {
  const path = (url ?? "").split("?")[0].replace(/\/+$/, "");

  return (
    path.endsWith("/auth/login") ||
    path.endsWith("/auth/register")
  );
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError<ApiErrorResponse>(error)) {
      return Promise.reject(error);
    }

    if (!error.response) {
      return Promise.reject(
        new ApiError(
          "Nem sikerült elérni a szervert. Ellenőrizd az internetkapcsolatot, majd próbáld újra."
        )
      );
    }

    const { status, data } = error.response;

    if (status === 401) {
      if (isLoginOrRegisterRequest(error.config?.url)) {
        return Promise.reject(
          new ApiError("Hibás e-mail-cím vagy jelszó.", status)
        );
      }

      try {
        await clearExpiredSession();
      } catch {
        // The in-memory session is reset even if storage cleanup fails.
      }

      return Promise.reject(
        new ApiError(
          "A munkameneted lejárt vagy érvénytelen. Jelentkezz be újra.",
          status
        )
      );
    }

    if (status === 403) {
      return Promise.reject(
        new ApiError(
          "Nincs jogosultságod ehhez a művelethez.",
          status
        )
      );
    }

    if (status === 429) {
      return Promise.reject(
        new ApiError(
          "Túl sok próbálkozás történt. Várj egy kicsit, majd próbáld újra.",
          status
        )
      );
    }

    if (status >= 500) {
      return Promise.reject(
        new ApiError(
          "A szerveren hiba történt. Próbáld újra később.",
          status
        )
      );
    }

    const responseBody =
      typeof data === "object" && data !== null ? data : null;

    const message =
      typeof responseBody?.message === "string"
        ? responseBody.message
        : typeof responseBody?.title === "string"
          ? responseBody.title
          : "A szerver nem tudta feldolgozni a kérést.";

    return Promise.reject(
      new ApiError(
        message,
        status,
        normalizeValidationErrors(responseBody?.errors)
      )
    );
  }
);