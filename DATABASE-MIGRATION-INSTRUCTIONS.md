# Database Migration Instructions

## Prerequisites
- PostgreSQL installed and running
- .NET SDK installed
- Project builds successfully without errors

## Step 1: Close Any Running Application Instances
First, make sure no instances of the application are running to avoid file locking issues:

```powershell
# Find and close any running instances
Get-Process -Name "FinalProject" -ErrorAction SilentlyContinue | Stop-Process -Force
```

## Step 2: Update Connection String
Ensure your database connection string in `appsettings.json` is correct:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=PustakBhandar;Username=postgres;Password=sabin"
}
```

Adjust the values as needed for your PostgreSQL installation.

## Step 3: Create Initial Migration
Navigate to the FinalProject directory and create the initial migration:

```powershell
cd ElibraryApp-main\FinalProject
dotnet ef migrations add InitialCreate
```

## Step 4: Apply Migration to Database
Apply the migration to create the database:

```powershell
dotnet ef database update
```

## Step 5: Verify Database
Connect to the PostgreSQL server and verify that the `PustakBhandar` database has been created with all the required tables.

## Troubleshooting

### Error: File locked by another process
If you get an error about a file being locked, make sure all instances of the application are closed:

```powershell
Get-Process -Name "FinalProject" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainModule.FileName -like "*FinalProject*" } | Stop-Process -Force
```

### Error: Database exists
If the database already exists but needs to be recreated:

1. Connect to PostgreSQL and drop the database:
   ```sql
   DROP DATABASE IF EXISTS "PustakBhandar";
   ```

2. Then run the migration commands again.

### Error: Unable to connect to PostgreSQL
Ensure PostgreSQL is running and accessible with the credentials specified in your connection string. 