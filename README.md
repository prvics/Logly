# Logly

Logly is a courier activity and expense tracking application built with ASP.NET Core and React Native.

It helps delivery couriers track their shifts, earnings, expenses, and fuel fill-ups, and understand how much they earn after recorded costs.

The frontend uses Expo to support development for iOS, Android, and the web.

## Project Status

Logly is under active development.

The authentication flow and an initial monthly dashboard are implemented. Data-entry screens and additional reporting features are still being developed.

## Features

### Implemented

- User registration and login
- Token-based authentication
- Current user profile retrieval
- Persistent local session storage:
  - Browser localStorage for web development
  - Expo SecureStore on iOS and Android
- Logout
- Monthly dashboard displaying:
  - Total revenue
  - Total expenses
  - Fuel costs
  - Work and other expenses
  - Net result after recorded expenses
  - Worked time in hours and minutes
  - Distance traveled
  - Delivered orders
  - Revenue and profit per hour
  - Revenue and profit per delivery
  - Cost per kilometer
- Bottom-tab navigation:
  - Overview
  - Data entry
  - Profile

### Backend API Coverage

The frontend API layer includes calls for:

- Authentication and current user information
- Vehicles
- Shifts
- Expenses
- Fuel fill-ups
- Dashboard summaries
- Dashboard timelines
- Dashboard breakdowns by delivery company and vehicle

API integration does not mean that every corresponding frontend screen is complete.

### Planned

- Shift creation and editing screens
- Expense creation and editing screens
- Fuel fill-up creation and editing screens
- Vehicle management screens
- Delivery company selection during shift entry
- Date-range and reporting filters
- Dashboard charts
- Improved mobile layouts and user experience
- Further authentication and session validation improvements

## How Logly Organizes Data

### Shifts

A shift records work performed for a delivery company using a vehicle.

Shift data includes:

- Date
- Delivery company
- Vehicle
- Worked hours and minutes
- Distance traveled
- Revenue
- Delivered orders
- Work expenses
- Optional notes

Revenue is currently recorded through shifts rather than a separate income-entry screen.

### Expenses

Expenses include an amount, date, and category, with optional links to a vehicle or delivery company.

Categories include food, drinks, parking, fuel, vehicle maintenance, phone, equipment, and other expenses.

### Fuel Fill-Ups

Fuel entries record:

- Vehicle
- Date
- Odometer reading
- Fuel volume
- Total price
- Whether the tank was filled completely
- Optional notes

### Dashboard

The dashboard presents the totals and calculated metrics returned by the backend for a selected period.

The initial frontend dashboard requests the current calendar month.

The displayed net result reflects the expenses included by the backend calculation. It should not be interpreted as a complete tax or accounting calculation.

## Tech Stack

### Backend

- C#
- .NET / ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Token-based authentication

The backend solution is organized into API, Application, Domain, and Infrastructure projects.

### Frontend

- React Native
- Expo
- TypeScript
- React Navigation
- Zustand
- Axios
- Expo SecureStore

## Repository Structure

```text
Logly/
├── backend/
│   ├── Logly.Api/
│   ├── Logly.Application/
│   ├── Logly.Domain/
│   └── Logly.Infrastructure/
├── frontend/
│   ├── App.tsx
│   ├── src/
│   │   ├── api/
│   │   ├── navigation/
│   │   ├── screens/
│   │   ├── storage/
│   │   ├── store/
│   │   └── types/
│   └── package.json
└── README.md
```

Additional test projects and configuration files may also be present.

## Running Locally

### Prerequisites

- A .NET SDK compatible with the backend project
- Node.js and npm compatible with the frontend dependencies
- Access to the configured development database
- Expo Go or an appropriate mobile development environment for device testing

### 1. Configure the Backend

Provide the database connection and authentication settings required by the backend configuration.

Keep credentials and signing secrets outside version control. Use local configuration, environment variables, or .NET user secrets as appropriate.

### 2. Start the Backend

From the repository root:

```powershell
cd backend/Logly.Api
dotnet run --urls "http://0.0.0.0:5000"
```

This starts the development API on port 5000 and makes it listen on all local network interfaces.

### 3. Install Frontend Dependencies

In a separate terminal, from the repository root:

```powershell
cd frontend
npm ci
```

### 4. Configure the Frontend API Address

Create a `.env` file in the `frontend` directory.

For web development on the same computer:

```env
EXPO_PUBLIC_API_LAN_IP=http://localhost:5000
```

Despite its name, this variable currently expects the complete API base URL, including the scheme and port.

Do not append `/api`: the endpoint paths already include that prefix.

For testing on a physical phone, replace `localhost` with the development computer's LAN IP address:

```env
EXPO_PUBLIC_API_LAN_IP=http://YOUR_COMPUTER_LAN_IP:5000
```

The phone must be able to reach the development computer and its backend port.

Restart Expo after changing the environment configuration.

### 5. Start the Frontend

```powershell
npx expo start
```

For web development, press `w` in the Expo terminal.

For mobile testing, open the project using a compatible Expo Go installation or development build.

To restart with a cleared Metro cache:

```powershell
npx expo start --clear
```

## Web Development and CORS

The frontend and backend use different development origins:

```text
Frontend: http://localhost:8081
Backend:  http://localhost:5000
```

The backend CORS configuration must allow the frontend origin.

If the frontend runs on a different host or port, update the development CORS policy accordingly.

Keep allowed origins explicit rather than allowing arbitrary origins in production.

## Authentication and Session Storage

The frontend stores the access token and current user data using a platform-specific storage layer:

| Platform | Storage |
| --- | --- |
| Web | Browser localStorage |
| iOS / Android | Expo SecureStore |

The application restores the saved local session on startup.

Restoring local data is not equivalent to verifying that a token is still valid. The backend remains responsible for authorizing protected API requests.

Browser localStorage is used for the current web implementation and does not provide the same protection as native secure storage. Production web authentication requires a separate security review.

Never commit or share passwords, access tokens, database credentials, or authentication signing keys.

## Development Checks

### Frontend Type Checking

From the `frontend` directory:

```powershell
npx tsc --noEmit
```

### Backend Build

From the `backend` directory:

```powershell
dotnet build
```

### Backend Tests

From the `backend` directory:

```powershell
dotnet test
```

## Configuration Files

Do not commit local `.env` files or configuration containing secrets.

A frontend `.env.example` can document the required variable without including private credentials:

```env
EXPO_PUBLIC_API_LAN_IP=http://localhost:5000
```

Variables prefixed with `EXPO_PUBLIC_` are exposed to the client application. They must not contain secrets.

## Roadmap

- [x] Registration and login
- [x] Platform-specific session storage
- [x] Session restoration
- [x] Initial monthly dashboard
- [x] Main tab navigation
- [x] Shift entry
- [ ] Shift editing
- [ ] Expense entry and editing
- [x] Fuel fill-up entry
- [ ] Fuel fill-up editing
- [ ] Vehicle management
- [ ] Reporting filters
- [ ] Dashboard charts
- [ ] Production readiness and security review
