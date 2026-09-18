import { apiClient } from "./client";
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  CurrentUserResponse,
  ExpenseResponse,
  CreateExpenseRequest,
  UpdateExpenseRequest,
  ShiftResponse,
  CreateShiftRequest,
  UpdateShiftRequest,
  FuelFillUpResponse,
  CreateFuelFillUpRequest,
  UpdateFuelFillUpRequest,
  VehicleResponse,
  CreateVehicleRequest,
  UpdateVehicleRequest,
  DashboardSummaryResponse,
  DashboardTimelineResponse,
  DashboardCompanyResponse,
  DashboardVehicleResponse,
  DashboardPeriod,
} from "../types/api";

// ---------- Auth ----------

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>(
    "/api/auth/login",
    request
  );
  return response.data;
}

export async function register(
  request: RegisterRequest
): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>(
    "/api/auth/register",
    request
  );
  return response.data;
}

export async function getCurrentUser(): Promise<CurrentUserResponse> {
  const response = await apiClient.get<CurrentUserResponse>("/api/auth/me");
  return response.data;
}

// ---------- Expenses ----------

export async function getExpenses(): Promise<ExpenseResponse[]> {
  const response = await apiClient.get<ExpenseResponse[]>("/api/expenses");
  return response.data;
}

export async function getExpenseById(id: string): Promise<ExpenseResponse> {
  const response = await apiClient.get<ExpenseResponse>(
    `/api/expenses/${id}`
  );
  return response.data;
}

export async function createExpense(
  request: CreateExpenseRequest
): Promise<ExpenseResponse> {
  const response = await apiClient.post<ExpenseResponse>(
    "/api/expenses",
    request
  );
  return response.data;
}

export async function updateExpense(
  id: string,
  request: UpdateExpenseRequest
): Promise<ExpenseResponse> {
  const response = await apiClient.put<ExpenseResponse>(
    `/api/expenses/${id}`,
    request
  );
  return response.data;
}

export async function deleteExpense(id: string): Promise<void> {
  await apiClient.delete(`/api/expenses/${id}`);
}

// ---------- Shifts ----------

export async function getShifts(
  userCompanyId: string
): Promise<ShiftResponse[]> {
  const response = await apiClient.get<ShiftResponse[]>("/api/shifts", {
    params: { userCompanyId },
  });
  return response.data;
}

export async function getShiftById(
  id: string,
  userCompanyId: string
): Promise<ShiftResponse> {
  const response = await apiClient.get<ShiftResponse>(`/api/shifts/${id}`, {
    params: { userCompanyId },
  });
  return response.data;
}

export async function createShift(
  request: CreateShiftRequest
): Promise<ShiftResponse> {
  const response = await apiClient.post<ShiftResponse>(
    "/api/shifts",
    request
  );
  return response.data;
}

export async function updateShift(
  id: string,
  request: UpdateShiftRequest
): Promise<ShiftResponse> {
  const response = await apiClient.put<ShiftResponse>(
    `/api/shifts/${id}`,
    request
  );
  return response.data;
}

export async function deleteShift(
  id: string,
  userCompanyId: string
): Promise<void> {
  await apiClient.delete(`/api/shifts/${id}`, {
    params: { userCompanyId },
  });
}

// ---------- Fuel fill-ups ----------

export async function getFuelFillUps(): Promise<FuelFillUpResponse[]> {
  const response = await apiClient.get<FuelFillUpResponse[]>(
    "/api/fuel-fill-ups"
  );
  return response.data;
}

export async function getFuelFillUpById(
  id: string
): Promise<FuelFillUpResponse> {
  const response = await apiClient.get<FuelFillUpResponse>(
    `/api/fuel-fill-ups/${id}`
  );
  return response.data;
}

export async function createFuelFillUp(
  request: CreateFuelFillUpRequest
): Promise<FuelFillUpResponse> {
  const response = await apiClient.post<FuelFillUpResponse>(
    "/api/fuel-fill-ups",
    request
  );
  return response.data;
}

export async function updateFuelFillUp(
  id: string,
  request: UpdateFuelFillUpRequest
): Promise<FuelFillUpResponse> {
  const response = await apiClient.put<FuelFillUpResponse>(
    `/api/fuel-fill-ups/${id}`,
    request
  );
  return response.data;
}

export async function deleteFuelFillUp(id: string): Promise<void> {
  await apiClient.delete(`/api/fuel-fill-ups/${id}`);
}

// ---------- Vehicles ----------

export async function getVehicles(): Promise<VehicleResponse[]> {
  const response = await apiClient.get<VehicleResponse[]>("/api/vehicles");
  return response.data;
}

export async function getVehicleById(id: string): Promise<VehicleResponse> {
  const response = await apiClient.get<VehicleResponse>(
    `/api/vehicles/${id}`
  );
  return response.data;
}

export async function createVehicle(
  request: CreateVehicleRequest
): Promise<VehicleResponse> {
  const response = await apiClient.post<VehicleResponse>(
    "/api/vehicles",
    request
  );
  return response.data;
}

export async function updateVehicle(
  id: string,
  request: UpdateVehicleRequest
): Promise<VehicleResponse> {
  const response = await apiClient.put<VehicleResponse>(
    `/api/vehicles/${id}`,
    request
  );
  return response.data;
}

export async function deleteVehicle(id: string): Promise<void> {
  await apiClient.delete(`/api/vehicles/${id}`);
}

// ---------- Dashboard ----------

export interface DashboardQueryParams {
  from?: string;
  to?: string;
  userCompanyId?: string;
  vehicleId?: string;
}

export async function getDashboardSummary(
  params: DashboardQueryParams
): Promise<DashboardSummaryResponse> {
  const response = await apiClient.get<DashboardSummaryResponse>(
    "/api/dashboard/summary",
    { params }
  );
  return response.data;
}

export async function getDashboardTimeline(
  params: DashboardQueryParams & { groupBy?: DashboardPeriod }
): Promise<DashboardTimelineResponse> {
  const response = await apiClient.get<DashboardTimelineResponse>(
    "/api/dashboard/timeline",
    { params }
  );
  return response.data;
}

export async function getDashboardByCompany(
  params: Omit<DashboardQueryParams, "userCompanyId">
): Promise<DashboardCompanyResponse[]> {
  const response = await apiClient.get<DashboardCompanyResponse[]>(
    "/api/dashboard/by-company",
    { params }
  );
  return response.data;
}

export async function getDashboardByVehicle(
  params: Omit<DashboardQueryParams, "vehicleId">
): Promise<DashboardVehicleResponse[]> {
  const response = await apiClient.get<DashboardVehicleResponse[]>(
    "/api/dashboard/by-vehicle",
    { params }
  );
  return response.data;
}