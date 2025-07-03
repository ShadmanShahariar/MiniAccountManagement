\# 💼 Mini Account Management System



A role-based accounting web application built using ASP.NET Core Razor Pages and SQL Server, designed to manage accounts, vouchers, and user permissions with a stored procedure–driven backend.



---



\## 🔧 Technologies Used



\- ✅ ASP.NET Core Razor Pages

\- ✅ ASP.NET Identity (with Custom Roles)

\- ✅ MS SQL Server (Stored Procedures Only)

\- ✅ No Entity Framework or LINQ



---



\## 📌 Core Features



\### 🔐 Authentication \& Authorization

\- Custom Roles: \*\*Admin\*\*, \*\*Accountant\*\*, \*\*Viewer\*\*

\- ASP.NET Identity used with stored procedure–based role and permission assignment

\- Role-based access control using custom permissions table and stored procedures

\- Dynamic sidebar menu renders based on assigned permissions per user



\### 🧑‍💼 User \& Permission Management

\- View list of all users

\- Assign multiple roles to users

\- Assign module-level permissions via stored procedures 

\- Stored permissions dynamically control frontend access



\### 📚 Chart of Accounts

\- Add, Update, Delete accounts

\- Parent-Child hierarchical account structure

\- Dropdown support for selecting parent accounts

\- Stored Procedure: `sp\_ManageChartOfAccounts`



\### 🧾 Voucher Entry Module

\- Support for \*\*Journal\*\*, \*\*Payment\*\*, and \*\*Receipt\*\* vouchers

\- Multi-line entry for Debit and Credit

\- Dropdown account selection with validation

\- View/Edit/Delete vouchers

\- Voucher Details page with breakdown

\- Stored Procedure: `sp\_SaveVoucher`



---



\## 🎯 Optional Features



\- \[ ] Export reports to Excel \*(Not implemented yet)\*



---



\## 🧪 How to Run



\### 🖥 Prerequisites

\- Visual Studio 2022+

\- SQL Server 2019 or higher

-Select razor page with identity(Authentication: Individual Accounts)

\- .NET SDK 6.0+

\- adminEmail = admin@local.com
\- adminPassword = Admin@123


\### 🔄 Setup Steps



1\. \*\*Clone the repository\*\*

&nbsp;   ```bash

&nbsp;   git clone https://github.com/ShadmanShahariar/MiniAccountManagement.git

&nbsp;   ```



2\. \*\*Open the solution in Visual Studio\*\*



3\. \*\*Update the connection string\*\* in `appsettings.json`

&nbsp;   ```json

&nbsp;   "ConnectionStrings": {

&nbsp; "DefaultConnection": "Server=.;Database=AccountManagementDB;Trusted\_Connection=True;TrustServerCertificate=True;"

&nbsp;   },

&nbsp;   ```



4\. \*\*Run SQL Scripts\*\*

&nbsp;   - Navigate to `stored procedures`

&nbsp;   - Run the following stored procedures:

&nbsp;       - `sp\_UpdateRoleModulePermission`

&nbsp;       - `sp\_ManageChartOfAccounts`

&nbsp;       - `sp\_SaveVoucher`

&nbsp;		 - `sp\_GetAllModulesWithPermissions`

&nbsp;       - Any seed data (Roles, Modules)



5\. \*\*Build and Run the project\*\*

&nbsp;   - Use IIS Express or Kestrel

&nbsp;   - Application will redirect to Login Page



---



\## 🖼 Screenshots



> ⚠ Add the screenshots inside the `screenshots/` folder and embed them here



| Login Page | Dashboard | Chart of Accounts | Voucher Entry | Voucher List |

|------------|-----------|-------------------|----------------|

| !\[Login](/screenshots/login.jpg) | !\[Dashboard](/screenshots/dashboard.jpg) | !\[Chart of Accounts](/screenshots/chartofaccountEntryandList.jpg) | !\[Voucher Entry](/screenshots/voucherEntry.jpg) | !\[Voucher List](/screenshots/voucherList.jpg) |


---


\## 🧠 Project Structure



MiniAccountManagement/

│

├── Pages/

│   ├── Accounts/

│   │   ├── \_ListPartial.cshtml

│   │   └── Chart.cshtml

│   │

│   ├── Admin/

│   │   └── Users.cshtml

│   │

│   ├── Security/

│   │   └── ModulePermissions.cshtml

│   │

│   ├── Shared/

│   │   ├── \_Layout.cshtml

│   │   ├── \_LoginPartial.cshtml

│   │   └── \_ValidationScriptsPartial.cshtml

│   │

│   ├── Vouchers/

│   │   ├── Entry.cshtml

│   │   ├── Index.cshtml

│   │   └── Details.cshtml

│

├── Stored procedures                 │

├── wwwroot/                     

├── appsettings.json

├── MiniAccountManagement.csproj

└── README.md





---



\## 📎 Stored Procedures List



| Procedure Name                    | Purpose                                                                 |

| --------------------------------- | ----------------------------------------------------------------------- |

| `sp\_UpdateRoleModulePermission`   | Assign and update module-level permissions for a given role             |

| `sp\_ManageChartOfAccounts`        | Insert, update, or soft delete chart of accounts with parent-child      |

| `sp\_SaveVoucher`                  | Save voucher header and details (debit/credit lines) in one transaction |

| `sp\_GetAllModulesWithPermissions` | Fetch all available modules along with the permissions by role/user     |



---



\## 🙋‍♂️ Author



\*\*👨‍💻 Shadmun Shahariar\*\*  

Full-Stack Developer (.NET + Vue.js)  

📧 Email: shadman352126@gmail.com  

🔗 GitHub: \[ShadmanShahariar](https://github.com/ShadmanShahariar)  



---



\## 🏁 Final Notes



\- The project follows a clean, structured approach with layered stored procedure–driven logic.

\- All logic for identity, permissions, and core features is handled without LINQ or Entity Framework.

\- This submission reflects my real-world experience in developing secure and maintainable enterprise applications.



---



\## 📬 Repository Link



🔗 \*\*https://github.com/ShadmanShahariar/MiniAccountManagement\*\*



---





