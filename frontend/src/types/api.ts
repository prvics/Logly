// ---------- Auth ----------

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// A /api/auth/register and /api/auth/login endpoint 
export interface AuthResponse {
  accessToken: string;
  user: CurrentUserResponse;
}

// A /api/auth/me endpoint
export interface CurrentUserResponse {
  id: string;
  email: string;
  displayName: string | null;
  createdAtUtc: string;
}

// ---------- Enums ----------

export enum ExpenseCategory {
  Food = 1,
  Drink = 2,
  Parking = 3,
  Fuel = 4,
  VehicleMaintenance = 5,
  Phone = 6,
  Equipment = 7,
  Other = 99,
}

export enum IncomeCategory {
  DeliveryEarnings = 1,
  Bonus = 2,
  Tip = 3,
  Adjustment = 4,
  Other = 99,
}

export enum VehicleType {
  Bicycle = 1,
  ElectricBicycle = 2,
  Scooter = 3,
  Motorcycle = 4,
  Car = 5,
  Other = 99,
}

export enum DashboardPeriod {
  Day = 1,
  Week = 2,
  Month = 3,
  Year = 4,
}

// display names in hungarian for now (for UI, in backend I use only numbers)

export const ExpenseCategoryLabels: Record<ExpenseCategory, string> = {
  [ExpenseCategory.Food]: "Étel",
  [ExpenseCategory.Drink]: "Ital",
  [ExpenseCategory.Parking]: "Parkolás",
  [ExpenseCategory.Fuel]: "Üzemanyag",
  [ExpenseCategory.VehicleMaintenance]: "Karbantartás",
  [ExpenseCategory.Phone]: "Telefon",
  [ExpenseCategory.Equipment]: "Felszerelés",
  [ExpenseCategory.Other]: "Egyéb",
};

export const IncomeCategoryLabels: Record<IncomeCategory, string> = {
  [IncomeCategory.DeliveryEarnings]: "Futár bevétel",
  [IncomeCategory.Bonus]: "Bónusz",
  [IncomeCategory.Tip]: "Borravaló",
  [IncomeCategory.Adjustment]: "Korrekció",
  [IncomeCategory.Other]: "Egyéb",
};

export const VehicleTypeLabels: Record<VehicleType, string> = {
  [VehicleType.Bicycle]: "Kerékpár",
  [VehicleType.ElectricBicycle]: "E-bike",
  [VehicleType.Scooter]: "Roller",
  [VehicleType.Motorcycle]: "Motor",
  [VehicleType.Car]: "Autó",
  [VehicleType.Other]: "Egyéb",
};

// ---------- Vehicles ----------

export interface CreateVehicleRequest {
  name: string;
  type: VehicleType;
  startingOdometerKm: number | null;
}

export interface UpdateVehicleRequest {
  name: string;
  type: VehicleType;
  isActive: boolean;
  startingOdometerKm: number | null;
}

export interface VehicleResponse {
  id: string;
  name: string;
  type: VehicleType;
  isActive: boolean;
  startingOdometerKm: number | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

// ---------- Shifts ----------
//  GetAll GetById and Delete enpoints needs userCompanyId
// query params needed

export interface CreateShiftRequest {
  userCompanyId: string;
  vehicleId: string;
  date: string; // yyyy-MM-dd
  workedHours: number;
  workedMinutes: number;
  drivenKilometers: number;
  revenue: number;
  deliveredOrders: number;
  workExpense: number;
  note: string | null;
}

export interface UpdateShiftRequest {
  userCompanyId: string;
  vehicleId: string;
  date: string;
  workedHours: number;
  workedMinutes: number;
  drivenKilometers: number;
  revenue: number;
  deliveredOrders: number;
  workExpense: number;
  note: string | null;
}

export interface ShiftResponse {
  id: string;
  userCompanyId: string;
  vehicleId: string;
  date: string;
  workedHours: number;
  workedMinutes: number;
  drivenKilometers: number;
  revenue: number;
  deliveredOrders: number;
  workExpense: number;
  note: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

// ---------- Fuel fill-ups ----------

export interface CreateFuelFillUpRequest {
  vehicleId: string;
  date: string;
  odometerKm: number;
  liters: number;
  totalPrice: number;
  isFullTank: boolean;
  note: string | null;
}

// UpdateFuelFillUpRequest do not have vehicleId!
export interface UpdateFuelFillUpRequest {
  date: string;
  odometerKm: number;
  liters: number;
  totalPrice: number;
  isFullTank: boolean;
  note: string | null;
}

export interface FuelFillUpResponse {
  id: string;
  vehicleId: string;
  date: string;
  odometerKm: number;
  liters: number;
  totalPrice: number;
  isFullTank: boolean;
  note: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

// ---------- Expenses ----------

export interface CreateExpenseRequest {
  vehicleId: string | null;
  userCompanyId: string | null;
  date: string; // yyyy-MM-dd
  amount: number;
  category: ExpenseCategory;
  isWorkRelated: boolean;
  note: string | null; // max 500 char
}

export interface UpdateExpenseRequest {
  vehicleId: string | null;
  userCompanyId: string | null;
  date: string;
  amount: number;
  category: ExpenseCategory;
  isWorkRelated: boolean;
  note: string | null;
}

export interface ExpenseResponse {
  id: string;
  vehicleId: string | null;
  userCompanyId: string | null;
  date: string;
  amount: number;
  category: ExpenseCategory;
  isWorkRelated: boolean;
  note: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

// ---------- Dashboard ----------

export interface DashboardSummaryResponse {
  from: string;
  to: string;
  userCompanyId: string | null;
  vehicleId: string | null;
  totalRevenue: number;
  totalFuelCost: number;
  totalWorkExpense: number;
  totalOtherExpense: number;
  totalExpense: number;
  netProfit: number;
  totalWorkedMinutes: number;
  totalWorkedHours: number;
  totalDrivenKilometers: number;
  totalDeliveredOrders: number;
  revenuePerHour: number;
  profitPerHour: number;
  revenuePerOrder: number;
  profitPerOrder: number;
  revenuePerKilometer: number;
  fuelCostPerKilometer: number;
  totalCostPerKilometer: number;
  includesFuelCost: boolean;
}

export interface DashboardTimelineItemResponse {
  periodStart: string;
  periodEnd: string;
  label: string;
  revenue: number;
  fuelCost: number;
  workExpense: number;
  otherExpense: number;
  totalExpense: number;
  netProfit: number;
  workedMinutes: number;
  drivenKilometers: number;
  deliveredOrders: number;
}

export interface DashboardTimelineResponse {
  from: string;
  to: string;
  groupBy: DashboardPeriod;
  userCompanyId: string | null;
  vehicleId: string | null;
  includesFuelCost: boolean;
  items: DashboardTimelineItemResponse[];
}

export interface DashboardCompanyResponse {
  userCompanyId: string;
  deliveryCompanyId: string;
  deliveryCompanyName: string;
  revenue: number;
  workExpense: number;
  otherExpense: number;
  totalExpense: number;
  netProfitBeforeFuel: number;
  workedMinutes: number;
  drivenKilometers: number;
  deliveredOrders: number;
  revenuePerHour: number;
  revenuePerOrder: number;
}

export interface DashboardVehicleResponse {
  vehicleId: string;
  vehicleName: string;
  revenue: number;
  fuelCost: number;
  workExpense: number;
  otherExpense: number;
  totalExpense: number;
  netProfit: number;
  workedMinutes: number;
  drivenKilometers: number;
  deliveredOrders: number;
  revenuePerHour: number;
  revenuePerOrder: number;
  fuelCostPerKilometer: number;
  totalCostPerKilometer: number;
}

// ---------- Generic API error shape ----------

export interface ApiErrorResponse {
  message?: string;
  errors?: Record<string, string[]>;
}

// ---------- Delivery companies ----------

export interface DeliveryCompanyResponse {
  id: string;
  name: string;
  isSystemDefined: boolean;
}

export interface CreateDeliveryCompanyRequest {
  name: string;
}

// ---------- User companies ----------

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

export interface CreateUserCompanyRequest {
  deliveryCompanyId: string;
  startedOn: string;
  note: string | null;
}

export interface UpdateUserCompanyRequest {
  startedOn: string;
  endedOn: string | null;
  isActive: boolean;
  note: string | null;
}