## 📦 BayWyn Couriers - Courier Management System (CMS)
A specialised desktop application suite built on the .NET Framework, designed for local logistics management. This system coordinates the workflow between logistics controllers and field couriers using a centralised service-oriented architecture.

## 🏛️ System Architecture
This project follows a N-Tier Architecture:

Client Layer (WPF): A rich desktop interface for Admin, Logistics Coordinator, and Courier roles, utilising XAML for a responsive UI.

Service Layer (WCF): A Windows Communication Foundation service that handles business logic and acts as the secure gateway between the clients and the database.

Data Layer (SQL Server): A relational database storing shipment data, staff logs, and system configurations.

## 🔑 Core Roles & Functionality

### 👔 Admin
Report Generation: Generate comprehensive PDF/Excel reports on delivery performance, courier efficiency, and monthly volume.

System Configuration: Manage system users, branch locations, and service rates.

Data Auditing: Access historical logs for all completed deliveries.

### 🛰️ Logistics Coordinator
Delivery Management: Monitor incoming delivery requests.

Assignment Engine: Manually assign or reassign parcels to specific couriers based on workload.

Queue Monitoring: Overview of pending, assigned, and overdue deliveries.

### 🚲 Courier
Shift Management: Controls to "Start Shift," "End Shift," and toggle "On Break" status.

Task List: View a personalised queue of assigned deliveries.

Status Updates: Mark parcels as "Picked Up" or "Completed" upon delivery.

## 🛠️ Technical Stack
UI Framework: WPF (Windows Presentation Foundation)

Communication: WCF (Windows Communication Foundation) - SOAP/TCP

Language: C#

Database: Microsoft SQL Server

ORM: Entity Framework (or ADO.NET)

## 📖 Wiki Content Overview
For more detailed documentation, refer to the Project Wiki, which includes:

WCF Service Contracts: Documentation of IService1 and Data Contracts.

SQL Schema: ER Diagrams for the Shipments, Staff, and WorkLogs tables. Includes Use Case diagram, Class diagram, BPMN diagram, State diagram and Sequence diagram.

Workflow Diagrams: Logical flow of a delivery from "Unassigned" to "Complete."

User Manuals: Specific guides for the Coordinator dashboard.
