import { apiClient } from "./client";

export interface UserCompanyResponse {
  id: string;
  deliveryCompanyId: string;
  deliveryCompanyName: string;
  isSystemDefined: boolean;
  startedOn: string;
  endedOn: string | null;
  isActive: boolean;
  note: string | null;
}

export async function getUserCompanies(): Promise<UserCompanyResponse[]> {
  const response = await apiClient.get<UserCompanyResponse[]>(
    "/api/user-companies"
  );

  return response.data;
}