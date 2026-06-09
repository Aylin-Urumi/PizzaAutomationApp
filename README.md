# 🍕 PizzaApp — Automation & Management System

A clean, modern, and high-performance desktop application built to streamline pizza shop operations. From order placement and accountability tracking to live inventory controls and staff workspace management, this system keeps the kitchen running smoothly.

---

## 🛠️ Tech Stack

*   **Framework:** .NET 10.0 (C#) using the **MVVM Pattern**
*   **UI Framework:** [Avalonia UI](https://avaloniaui.net/) (Modern, cross-platform XAML styling)
*   **Database:** SQLite
*   **ORM:** Entity Framework Core (EF Core)
*   **Mvvm Architecture:** CommunityToolkit.Mvvm (Source generators, `[RelayCommand]`)

---

## 🚀 Core Features

### 1. 📊 Sales & Performance Metrics
*   **Live Financial Trackers:** High-visibility trackers for Gross Sales, Daily Revenue, and Monthly Revenue.
*   **Order History Matrix:** Full audit log showing timestamps, customers, execution statuses, and total prices at a glance.

### 2. 👤 Multi-Role Accountability Tracking
Every order acts as a permanent ledger, tracking exactly who processed it at every step of the workflow:
*   **Cashier:** The operator who took the order and finalized payment.
*   **Chef:** The specific kitchen worker assigned to prepare the meal.
*   **Driver:** The courier dispatching delivery orders.

### 3. 🔐 Staff & Access Management
*   **Role Provisioning:** Create specialized user accounts (Manager, Cashier, Chef, Driver) with unique login PIN credentials.
*   **Safe Revoke Security:** Safely delete employee records. Past historical orders automatically unlink their reference IDs to protect the integrity of the data ledger instead of throwing database foreign key constraint violations.

### 4. 📦 Supply Chain & Inventory Control
*   **Live Monitoring:** View raw ingredient quantities dynamically weighed in real-time.
*   **Warehouse Inflows:** Built-in restock panel allowing managers to execute bulk-supply shipments instantly.

### 5. 💬 Customer Feedback Dashboard
*   **Performance Logs:** Dedicated review panel for monitoring active customer experience reports.

---

## 🏁 Getting Started

### Prerequisites
Make sure you have the latest **.NET SDK** installed on your machine.

### Installation & Launch

1. **Clone the repository:**
```bash
   git clone [https://github.com/yourusername/PizzaAutomation.git](https://github.com/yourusername/PizzaAutomation.git)
   cd PizzaAutomation/PizzaApp
2. **Restore dependencies:**
```bash
   dotnet restore
3. 