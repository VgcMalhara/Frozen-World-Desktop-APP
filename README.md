# Frozen World POS System - Desktop Frontend Client

A modern, high-performance desktop Point of Sale (POS) and inventory management client developed for **Frozen World**. Built with **C# and Windows Presentation Foundation (WPF)** on modern .NET, this application provides an intuitive SaaS-grade user experience for managing ice cream flavors, stock levels, and customer records while interfacing securely with a remote REST API backend.

> **Note:** The backend API (built with Laravel) and database are hosted and maintained in a separate repository. This repository contains the standalone desktop frontend client.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Key Features](#key-features)
  - [Role-Based Access Control (RBAC)](#role-based-access-control-rbac)
  - [API Authorization & State Management](#api-authorization--state-management)
  - [Inventory & Catalog Management](#inventory--catalog-management)
  - [Customer Management (CRM)](#customer-management-crm)
  - [Modern UI/UX Design System](#modern-uiux-design-system)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Setup & Installation](#setup--installation)
  - [Prerequisites](#prerequisites)
  - [Running via Visual Studio](#running-via-visual-studio)
  - [Running via .NET CLI](#running-via-net-cli)
  - [Configuring the API Endpoint](#configuring-the-api-endpoint)
- [API Testing](#api-testing)

---

## Project Overview

The **Frozen World POS Desktop Client** provides a secure, responsive desktop interface designed for frozen dessert retail operations. It communicates with a remote Laravel REST API over HTTP using JSON payloads and Bearer token authentication.

### Highlights:
- **Centralized API Service:** Dedicated `ApiService` encapsulates all HTTP communication (`HttpClient`) with asynchronous methods (`async`/`await`), automatic authorization header injection, and robust error handling.
- **Resilient Network Handling:** Handles HTTP status codes seamlessly (including `401 Unauthorized` for expired sessions and `422 Unprocessable Entity` for validation errors).
- **Zero UI Freezing:** All network and data-processing operations execute asynchronously off the UI thread with interactive loading indicators and disabled action triggers to prevent duplicate submissions.

---

## Key Features

### Role-Based Access Control (RBAC)

The application implements granular Role-Based Access Control strictly aligned with the assessment specification. Instead of artificially hiding buttons, all primary action buttons (`Add`, `Edit`, `Delete`) remain discoverable across all authenticated roles, enforcing permissions directly at the action level and presenting explicit feedback:

| Role | Inventory Permissions | Customer Permissions | Behavior on Restricted Action |
| :--- | :--- | :--- | :--- |
| **Owner / Admin** | Add, View, Edit, Delete | Add, View, Edit, Delete | Full unrestricted access. |
| **Manager** | Add, View, Edit | Add, View, Edit | Clicking **Delete** on items or customers displays an **Access Denied** permission dialog. |
| **Cashier** | View, Edit | Add, View, Edit | Clicking **+ Add New Flavor** or **Delete** displays an **Access Denied** permission dialog. |

#### RBAC Error Feedback Example:
When an unauthorized user triggers a restricted action, the application blocks the request before it leaves the client:
```text
Title:   Permission Error
Message: Access Denied: You do not have permission to perform this action.
Icon:    Error (Hand)
```

### API Authorization & State Management
- **Bearer Token Lifecycle:** On successful login (`/api/auth/login`), the Bearer token and current user profile (including role) are stored in memory in `ApiService`.
- **Automatic Authorization Injection:** The `HttpClient` default request headers are configured with `Authorization: Bearer {token}` for all subsequent API requests.
- **Graceful Session Termination:** Logging out clears tokens, resets user state, and routes the application securely back to `LoginWindow`.

### Inventory & Catalog Management
- **Live Search & Filter:** Instant real-time filtering across flavor names, categories, IDs, and prices.
- **Stock Tracking:** Color-coded status badges for real-time stock levels.
- **Full CRUD:** Modal forms for creating new flavors and modifying pricing or inventory quantities, complete with input validation and confirmation dialogs for deletions.
- **Empty States:** Visual placeholder cards when no items match search criteria or when the catalog is empty.

### Customer Management (CRM)
- **Customer Directory:** View customer profiles, telephone numbers, and email communication history.
- **CRUD Operations:** Dedicated popup dialogs for adding and editing customer details.
- **Validation Handling:** Directly surfaces backend 422 validation errors (e.g., duplicate email or phone formats) into intuitive UI error blocks.

### Modern UI/UX Design System
- Unified design system configured in [`App.xaml`](App.xaml) using a cohesive color palette (Primary, Surface, Success, Danger, Warning).
- Custom borderless `DataGrid` styles with hover transitions and rounded corners.
- Floating placeholder watermarks and active focus rings for input text boxes.
- Smooth page navigation transitions and animated loading spinners.

---

## Tech Stack

- **Platform:** Windows Desktop Application
- **Language:** C#
- **UI Framework:** Windows Presentation Foundation (WPF)
- **Runtime:** .NET 8 / .NET 10 (`Microsoft.NET.Sdk`)
- **HTTP Client:** `System.Net.Http.HttpClient` with `System.Net.Http.Json`
- **Data Serialization:** `System.Text.Json`
- **Architecture:** Code-Behind with Service-Layer Separation (`ApiService`)

---

## Project Structure

```text
FrozenWorld/
├── Models/                     # Data transfer objects and API models
│   ├── ApiResponse.cs          # Generic JSON response wrapper
│   ├── AuthResponse.cs         # Authentication response model
│   ├── Customer.cs             # Customer entity
│   ├── InventoryItem.cs        # Inventory item model
│   └── User.cs                 # Authenticated user model & role definition
├── Services/                   # Core business and API logic
│   └── ApiService.cs           # HTTP client, token management, and REST calls
├── Views/                      # WPF Pages, Windows, and Custom Dialogs
│   ├── AddCustomerWindow.xaml  # Add customer popup dialog
│   ├── AddItemWindow.xaml      # Add flavor popup dialog
│   ├── CustomerPage.xaml       # Customer management page
│   ├── EditCustomerWindow.xaml # Edit customer modal dialog
│   ├── EditItemWindow.xaml     # Edit flavor modal dialog
│   ├── InventoryPage.xaml      # Inventory catalog page with live search
│   ├── LoginWindow.xaml        # User authentication window
│   ├── MainWindow.xaml         # Main shell with sidebar navigation & user badge
│   └── ModernDialog.xaml       # Custom styled message and confirmation dialogs
├── App.xaml                    # Global ResourceDictionary, color palette, styles
├── App.xaml.cs                 # Application bootstrap
├── favicon.ico                 # Application window and system icon
├── FrozenWorld.csproj          # Project configuration file
└── README.md                   # Project documentation
```

---

## Setup & Installation

### Prerequisites

Ensure you have the following installed on your Windows development environment:
1. **Windows 10 or Windows 11**
2. **Visual Studio 2022** (v17.8 or newer) with the **.NET desktop development** workload selected, or the **.NET SDK (8.0 or 10.0)**.
3. A running instance of the **Frozen World Laravel API** backend (default: `http://127.0.0.1:8000`).

---

### Running via Visual Studio

1. **Open the Project:**
   - Launch Visual Studio 2022.
   - Click **Open a project or solution**.
   - Browse to the project root directory and select `FrozenWorld.csproj` (or open the root folder).

2. **Restore NuGet Packages:**
   - Right-click the `FrozenWorld` project in the **Solution Explorer**.
   - Select **Restore NuGet Packages** (Visual Studio will automatically resolve SDK-level references including `System.Net.Http.Json`).

3. **Build the Solution:**
   - From the top menu, click **Build** > **Build Solution** (or press `Ctrl + Shift + B`).
   - Verify that the Output window shows `Build: 1 succeeded, 0 failed`.

4. **Run the Application:**
   - Press `F5` (Start Debugging) or `Ctrl + F5` (Start Without Debugging).
   - The application will launch with the `LoginWindow`.

---

### Running via .NET CLI

Alternatively, you can build and launch the application directly using the command line:

```powershell
# 1. Navigate to the project root
cd path\to\FrozenWorld

# 2. Restore project dependencies
dotnet restore

# 3. Build the project
dotnet build

# 4. Run the WPF desktop client
dotnet run
```

---

### Configuring the API Endpoint

By default, the desktop client connects to the local development backend:
```csharp
// Services/ApiService.cs
_httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/api/");
```

If your backend is hosted remotely or running on a different port/IP address, update the `BaseAddress` in [`Services/ApiService.cs`](Services/ApiService.cs) before building the project.

---

## API Testing

A complete Postman collection is included in the project files to facilitate backend testing and API exploration:

- **Collection File:** `Frozen_World_API.postman_collection.json`

### Highlights & Automated Token Management:
- **Dynamic Variable Injection:** The collection includes pre-configured **Tests scripts** on the `auth/login` endpoint that automatically extract the Bearer token from the response JSON and set it as an active environment/collection variable (`{{token}}`).
- **Zero-Manual Copy-Paste:** All subsequent protected endpoints (`/items`, `/customers`) automatically reference `{{token}}` in their Authorization header:
  ```text
  Authorization: Bearer {{token}}
  ```
- **Complete Endpoint Coverage:** Covers full CRUD operations for both Inventory and Customers, login authentication, and validation testing (422 responses).

#### To import into Postman:
1. Open Postman.
2. Click **Import** in the upper left corner.
3. Select or drag-and-drop `Frozen_World_API.postman_collection.json`.
4. Run the `Login` request first; all subsequent requests will authenticate automatically.

