# Logly

Logly is a courier activity and expense tracking application built with ASP.NET Core and React Native.

It helps delivery couriers track their shifts, earnings, expenses, and fuel fill-ups, and understand how much they earn after recorded costs.

The frontend uses Expo to support development for iOS, Android, and the web.

## Project Status

Logly is under active development.

Authentication, an initial monthly dashboard, shift entry, fuel fill-up entry, and delivery company relationship management are implemented.

The frontend includes selectable light and dark themes, themed navigation, and loading skeletons for the main data-loading sections.

Additional management screens, editing workflows, reporting features, and production hardening are still being developed.

## Features

### Authentication and Sessions

- User registration and login
- Token-based authentication
- Current user profile retrieval
- Persistent local session storage:
  - Browser localStorage for web development
  - Expo SecureStore on iOS and Android
- Local session restoration on startup
- Logout

### Monthly Dashboard

The dashboard displays:

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
- The reporting period
- A notice when fuel costs are not included because there is insufficient fuel data

The initial frontend dashboard requests the current calendar month.

### Shift Entry

- Active delivery company relationship selection
- Active vehicle selection
- Shift date
- Worked hours and minutes
- Revenue
- Delivered orders
- Distance traveled
- Shift-related work expenses
- Optional notes
- Client-side validation
- Backend validation feedback
- Submission progress and success feedback

The shift date is validated against the selected delivery company relationship's start and end dates.

### Fuel Fill-Up Entry

- Active vehicle selection
- Fill-up date
- Odometer reading
- Fuel volume
- Total price
- Full-tank toggle
- Optional notes
- Client-side validation
- Backend validation feedback
- Submission progress and success feedback

### Profile and Delivery Companies

- Display name and email
- List of the user's delivery company relationships
- Active and inactive relationship status
- Add an existing delivery company to the profile
- Create a delivery company in the shared company list
- Edit relationship start and end dates
- Edit relationship notes
- Inactivate or reactivate a relationship

Creating a shared delivery company does not automatically create a user relationship. The company must then be added through the relationship form.

Inactivating a relationship does not delete historical shifts or revenue.

### Appearance and Navigation

- Bottom-tab navigation:
  - Overview
  - Data entry
  - Profile
- Selectable light and dark themes
- Animated theme switch in the Profile appearance section
- Persistent local theme preference
- Centralized theme colors
- Themed navigation bar and navigation headers
- Themed shared choice fields, text inputs, and date picker
- A theme registry that can be extended later

Only light and dark themes are currently available.

### Loading States and Transitions

Layout-specific skeleton placeholders are implemented for:

- Dashboard summaries and metric cards
- Shift entry options and form layout
- Fuel fill-up options and form layout
- Profile delivery company cards and forms

Skeleton visibility follows actual API loading state rather than an artificial timer.

Loading errors provide feedback and retry actions. Empty results are handled separately from loading and error states.

The interface also includes:

- Animated selection between shift and fuel entry
- Reusable fade-in transitions
- A configurable vertical offset for content appearance
- Dashboard fade-in without vertical movement
- Theme-aware skeleton colors

List sizes are not known before the first API response. Initial skeletons therefore use an estimated item count; subsequent reloads can reuse the previous non-empty list count.

Optional text, different item counts, and dynamic content may still produce differences between placeholder and final content height.

### Backend API Coverage

The frontend API layer includes calls for:

- Authentication and current user information
- Vehicles
- Shifts
- Expenses
- Fuel fill-ups
- Delivery companies
- User delivery company relationships
- Dashboard summaries
- Dashboard timelines
- Dashboard breakdowns by delivery company and vehicle

API integration does not mean that every corresponding frontend screen is complete.

### Planned

- Shift editing
- Expense creation and editing screens
- Fuel fill-up editing
- Vehicle management screens
- Date-range and reporting filters
- Dashboard charts
- Further layout and accessibility refinements
- Further authentication and session validation improvements
- Additional themes, if needed
- Production readiness and security review

## How Logly Organizes Data

### Shifts

A shift records work performed through a user's delivery company relationship using a vehicle.

Shift data includes:

- Date
- Delivery company relationship
- Vehicle
- Worked hours and minutes
- Distance traveled
- Revenue
- Delivered orders
- Work expenses
- Optional notes

Revenue is currently recorded through shifts rather than a separate income-entry screen.

### Delivery Company Relationships

The application distinguishes between:

- A delivery company in the shared company list
- A user's relationship with that delivery company

A user relationship includes:

- Delivery company
- Start date
- Optional end date
- Active or inactive status
- Optional notes

The shared company name is not editable through the user relationship form.

### Expenses

Expenses include an amount, date, and category, with optional links to a vehicle or delivery company.

Categories include food, drinks, parking, fuel, vehicle maintenance, phone, equipment, and other expenses.

A dedicated frontend expense-entry workflow is still planned.

### Fuel Fill-Ups

Fuel entries record:

- Vehicle
- Date
- Odometer reading
- Fuel volume
- Total price
- Whether the tank was filled completely
- Optional notes

Avoid recording the same fuel cost again as a separate expense or shift work expense, because the current dashboard can count these recorded costs together.

The same principle applies to other work expenses: do not record the same cost through both a shift and a separate expense.

### Dashboard

The dashboard presents totals and calculated metrics returned by the backend for a selected period.

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
- AsyncStorage for the theme preference
- React Native Calendars
- React Native Animated API

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
│   │   ├── components/
│   │   │   ├── capture/
│   │   │   ├── dashboard/
│   │   │   ├── profile/
│   │   │   └── ui/
│   │   ├── navigation/
│   │   ├── screens/
│   │   ├── storage/
│   │   ├── store/
│   │   ├── theme/
│   │   └── types/
│   └── package.json
└── README.md
```

Additional test projects and configuration files may also be present.

## Frontend UI Organization

### Themes

The `src/theme/` directory contains:

- Theme color definitions
- The theme registry
- Theme context and provider
- Local theme preference persistence

The theme provider wraps the application navigation so that screens and shared components use the same selected theme.

### Shared UI Components

The `src/components/ui/` directory includes reusable components such as:

- ChoiceField
- DateField
- FormField
- PrimaryButton
- FeedbackMessage
- Skeleton
- FadeInView
- ThemeSwitch

### Loading Skeletons

Feature-specific skeletons are kept near their related components:

- `components/dashboard/DashboardSkeleton.tsx`
- `components/capture/ShiftFormSkeleton.tsx`
- `components/capture/FuelFillUpFormSkeleton.tsx`
- `components/capture/FormSkeletonParts.tsx`
- `components/profile/UserCompaniesSkeleton.tsx`

When changing a screen layout, review its skeleton as well. Field heights, spacing, card structure, and responsive wrapping should remain aligned with the final content.

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

The theme preference is stored separately using AsyncStorage. It is a UI preference, not an authentication credential.

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

### Manual UI Checks

Check the following when updating screens or shared components:

- Light and dark theme appearance
- Theme preference after reloading the application
- Navigation bar colors in both themes
- Form labels, entered values, placeholders, and validation errors
- Date picker appearance
- Skeleton-to-content transitions
- Slow-network loading behavior
- Empty states and failed-request retry actions
- Form values when switching between shift and fuel entry
- Layout on narrow and wide viewports

The loading interfaces have been manually reviewed in browser mobile emulation with 3G network throttling.

This does not replace testing on physical Android and iOS devices, nor does it imply that automated checks have passed.

Some animations respect the system reduced-motion preference. Check this setting when an expected animation does not appear.

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
- [x] Delivery company selection during shift entry
- [x] Fuel fill-up entry
- [x] Profile delivery company relationship management
- [x] Shared delivery company creation
- [x] Selectable light and dark themes
- [x] Persistent theme preference
- [x] Themed navigation
- [x] Themed shared choice, input, and date fields
- [x] API-driven loading skeletons for main data-loading sections
- [x] Reusable UI transitions
- [ ] Shift editing
- [ ] Expense entry and editing
- [ ] Fuel fill-up editing
- [ ] Vehicle management
- [ ] Reporting filters
- [ ] Dashboard charts
- [ ] Further accessibility and layout refinements
- [ ] Physical-device UI verification
- [ ] Production readiness and security review
