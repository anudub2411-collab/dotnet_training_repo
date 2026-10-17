# ResourceManagementSystem

## How to run the app
1. Ensure .NET 10 SDK is installed.
2. From solution root, run:
   - `dotnet restore`
   - `dotnet run --project ResourceMangementSystem/ResourceMangementSystem.csproj`
3. Use the console menu to manage employees, projects, allocations, matching, and reports.

Run tests:
- `dotnet test`

## Features implemented
- Employee management: add, update, deactivate.
- Skill management: add/update employee skills with proficiency levels (1-5 enum).
- Project management: create and manage projects with required skills.
- Allocation management with business rules:
  - Total overlapping allocation <= 100%.
  - Inactive employees cannot be allocated.
  - Allocation dates must fall within project dates.
- Matching engine: suggest available employees for project requirements.
- Reports:
  - Bench report (below user-defined threshold).
  - Project staffing summary.
  - Utilization per department.
- JSON persistence through abstraction (`App_data/EmpRecord.json`).
- Simulated notifications on allocation and release.
- Composition root in `Program.cs` only.

## Sample console session
```text
Resource Management System
1. List employees
2. Add employee
...
Choose option: 8
Employee ID: 11111111-1111-1111-1111-111111111012
Project ID: 22222222-2222-2222-2222-222222222012
Allocation start date (yyyy-MM-dd): 2026-06-01
Allocation end date (yyyy-MM-dd): 2026-09-30
Allocation percentage: 40
[NOTIFY] Allocated Nikhil Rao to Comet Intern Pilot (40%) from 2026-06-01 to 2026-09-30
Allocation created: 5c45bf2e-9f35-4b59-9e8f-4f4925ed7d59

Choose option: 11
Bench threshold percentage: 50
Nikhil Rao | Engineering | 40%
Aarav Sharma | Engineering | 30%
```
