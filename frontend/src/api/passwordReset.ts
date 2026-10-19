import { apiClient } from "./client";

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface PasswordResetMessageResponse {
  message: string;
}

export async function forgotPassword(
  request: ForgotPasswordRequest
): Promise<PasswordResetMessageResponse> {
  const response =
    await apiClient.post<PasswordResetMessageResponse>(
      "/api/auth/forgot-password",
      request
    );

  return response.data;
}

export async function resetPassword(
  request: ResetPasswordRequest
): Promise<PasswordResetMessageResponse> {
  const response =
    await apiClient.post<PasswordResetMessageResponse>(
      "/api/auth/reset-password",
      request
    );

  return response.data;
}