# .NET Console Application with PostgreSQL

## Overview
This .NET console application demonstrates fetching JSON data from a remote URL, deserializing it into C# objects, and storing it in a PostgreSQL database. The application then reads from the database and outputs sorted results to the console.

## Table Schema
The application uses a PostgreSQL database table that has the following schema.  The table name is configurable and should be set in appsettings.json: `"Postgres:TableName".`

| Column Name | Data Type       | Constraints   |
|-------------|-----------------|---------------|
| `Id`        | `TEXT`          | `PRIMARY KEY` |
| `FirstName` | `TEXT`          | `NOT NULL`    |
| `LastName`  | `TEXT`          | `NOT NULL`    |
| `Language`  | `TEXT`          | `NOT NULL`    |
| `Bio`       | `TEXT`          | `NOT NULL`    |
| `Version`   | `NUMERIC(10, 2)` | `NOT NULL`   |


## Installation
### 1. Clone the Repository
```
git clone https://github.com/mrjordantanner/boostlingo-app.git
cd boostlingo-app
```
### 2. Install Dependencies
```
dotnet restore
```
### 3. Download & Install PostgreSQL
Install from  **[Postgres Official Downloads](https://www.postgresql.org/download/)**.  You can use the CLI or the pgAdmin 4 UI to set up Postgres.
### 4. Set up PostgresSQL using CLI
Open a Terminal and run the PostgreSQL interactive terminal (psql):
```
psql -U postgres
```
Set up the database and user:
```
CREATE DATABASE test-db;
CREATE USER test-user WITH PASSWORD 'your-password';
GRANT ALL PRIVILEGES ON DATABASE test-db TO test-user;
```
Connect to the Database:
```
\c test-db
```
Create the Table:
```
CREATE TABLE your-table-name (
    Id TEXT PRIMARY KEY,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Language TEXT NOT NULL,
    Bio TEXT NOT NULL,
    Version NUMERIC(10, 2) NOT NULL
);
```

## Configuration
Create an **appsettings.json** file in the root directory and change the values as appropriate.
```
{
  "AppSettings": {
    "DataUrl": "https://microsoftedge.github.io/Demos/json-dummy-data/64KB.json"
  },
  "Postgres": {
    "TableName": "your-table-name",
    "ConnectionString": "Host=localhost;Port=5432;Username=test-user;Password=your-password;Database=test-db"
  }
}
```

## Running the Application

```
dotnet build
dotnet run
```