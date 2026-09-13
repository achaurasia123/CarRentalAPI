# Car Rental Booking API (.NET 10 + EF Core + SQL Server)

Code-First Entity Framework Core backend, JWT authentication, table naming convention:
- `mst_` = master tables
- `trn_` = transaction tables
- `tbl_` = other / helper tables

## Requirements
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB / Express / full instance)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

## Database tables created

| Type   | Table               | Purpose                                   |
|--------|---------------------|--------------------------------------------|
| mst_   | mst_role            | User roles (Admin, Customer)               |
| mst_   | mst_car_type        | Car categories (Sedan, SUV, ...)           |
| mst_   | mst_location        | Cities where cars are available            |
| mst_   | mst_user            | Registered users / login credentials       |
| mst_   | mst_car             | Car catalog (main master table)            |
| trn_   | trn_booking         | Car booking transactions                   |
| tbl_   | tbl_refresh_token   | JWT refresh tokens (helper table)          |

## Setup

### 1. Configure connection string
Edit `CarRentalAPI/appsettings.json` (or `appsettings.Development.json`) and set your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=CarRentalDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Also change the `Jwt:Key` value in `appsettings.json` to your own long random secret (min 32 characters) before deploying anywhere real.

### 2. Restore packages
```bash
cd CarRentalAPI
dotnet restore
```

### 3. Create the database — two options

**Option A: EF Core Migrations (recommended, Code-First)**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
This reads the entity classes in `Entities/` + `Data/AppDbContext.cs` and generates/applies the migration, creating all `mst_`, `trn_`, `tbl_` tables with seed data automatically.

**Option B: Run the raw SQL script directly**
Open `Database/schema.sql` (in the repo root, one level above `CarRentalAPI/`) in SQL Server Management Studio / Azure Data Studio and execute it. This creates the same schema + seed data without needing the EF CLI.

> Use Option A if you want the schema to be regenerated automatically whenever you change entity classes later. Use Option B if you just want the DB ready immediately.

### 4. Run the API
```bash
dotnet run
```
Swagger UI opens automatically at `https://localhost:7001/swagger` (or `http://localhost:5000/swagger`).

## Demo login
```
Email:    demo@driveon.com
Password: Demo@123
```

## API Endpoints

| Method | Endpoint                     | Auth required | Description                     |
|--------|-------------------------------|----------------|----------------------------------|
| POST   | /api/auth/register            | No             | Register new user                |
| POST   | /api/auth/login               | No             | Login, returns JWT token         |
| GET    | /api/cars                     | No             | List cars (`?search=&carType=`)  |
| GET    | /api/cars/{id}                | No             | Get single car                   |
| GET    | /api/cars/types                | No             | List car type names              |
| GET    | /api/bookings                 | Yes (Bearer)   | Logged-in user's bookings         |
| POST   | /api/bookings                 | Yes (Bearer)   | Create a booking                  |
| PUT    | /api/bookings/{id}/cancel      | Yes (Bearer)   | Cancel a booking                  |
| GET    | /api/dashboard/stats           | Yes (Bearer)   | Dashboard summary stats           |

All protected endpoints need header: `Authorization: Bearer <token>` (token returned from `/api/auth/login`).

## CORS
Angular dev server (`http://localhost:4200`) is already allowed in `Program.cs` under the `AllowAngularApp` policy. Update this if your Angular app runs on a different port/domain.

## Adding new migrations later
Whenever you change an entity class:
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
