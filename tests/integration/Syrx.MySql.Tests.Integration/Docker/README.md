# Syrx MySQL Docker Integration Tests

This directory contains Docker infrastructure for running Syrx MySQL integration tests using a containerized MySQL database.

## Overview

The Docker setup provides a consistent, isolated MySQL environment for integration testing that mirrors the approach used in Syrx.SqlServer and Syrx.Npgsql. This ensures consistency across all Syrx database provider implementations.

## Files Structure

```
Docker/
├── docker-compose.yml          # Docker Compose configuration
├── Dockerfile                  # MySQL container definition
├── init-scripts/              # Database initialization scripts
│   ├── 00-grant-privileges.sql # User privilege setup
│   ├── 01-setup-database.sql   # Database setup
│   ├── 02-create-tables.sql    # Test table creation
│   ├── 03-create-procedures.sql # Stored procedures
│   └── 04-seed-data.sql        # Test data seeding
├── build-mysql-image.ps1       # Image build script
└── README.md                   # This file
```

## Prerequisites

- Docker Desktop or Docker Engine installed
- Docker Compose available
- PowerShell (for Windows users)

## Starting the Test Database

### Using Docker Compose

```bash
# Navigate to the Docker directory
cd tests/integration/Syrx.MySql.Tests.Integration/Docker

# Start the MySQL container
docker-compose up -d

# Check container status
docker-compose ps

# View logs
docker-compose logs mysql
```

### Using PowerShell (Windows)

```powershell
# Navigate to the Docker directory
Set-Location "tests\integration\Syrx.MySql.Tests.Integration\Docker"

# Start the container
docker-compose up -d
```

## Connection Details

The MySQL container is configured with the following connection details:

- **Host**: localhost
- **Port**: 3306
- **Database**: syrx
- **Username**: syrx_user
- **Password**: YourStrong!Passw0rd
- **Root Password**: YourStrong!Passw0rd
- **Connection String**: `Server=localhost;Port=3306;Database=syrx;Uid=syrx_user;Pwd=YourStrong!Passw0rd;`

## Database Schema

The initialization scripts create the following tables:

### Tables
- `poco` - Main test table with id (INT AUTO_INCREMENT), name (VARCHAR), value (DECIMAL), modified (DATETIME)
- `identity_test` - Identity testing table with same structure
- `bulk_insert` - Bulk operations table with same structure  
- `distributed_transaction` - Distributed transaction testing table with same structure

### Stored Procedures
- `usp_create_table(table_name)` - Dynamic table creation procedure
- `usp_identity_tester(name, value)` - Identity value testing procedure
- `usp_bulk_insert(table_name)` - Bulk data insertion procedure
- `usp_bulk_insert_and_return(table_name)` - Bulk insert with return values
- `usp_clear_table(table_name)` - Table truncation procedure

## Running Integration Tests

Once the MySQL container is running, you can execute the integration tests:

```bash
# Run all integration tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal
```

## Health Checks

The container includes health checks that verify MySQL is ready to accept connections:

```bash
# Check container health
docker-compose ps

# Manual health check
docker exec syrx-mysql-tests mysqladmin ping -h localhost -u root -pYourStrong!Passw0rd
```

## Troubleshooting

### Container Won't Start
1. Check if port 3306 is already in use: `netstat -an | findstr 3306`
2. Ensure Docker Desktop is running
3. Check Docker logs: `docker-compose logs mysql`

### Connection Issues
1. Verify container is healthy: `docker-compose ps`
2. Test connection manually: `docker exec -it syrx-mysql-tests mysql -u syrx_user -pYourStrong!Passw0rd syrx`
3. Check firewall settings if connecting from external machine

### Database Issues
1. Check initialization logs: `docker-compose logs mysql`
2. Connect to database and verify schema: `docker exec -it syrx-mysql-tests mysql -u syrx_user -pYourStrong!Passw0rd syrx -e "SHOW TABLES;"`
3. Verify test data: `docker exec -it syrx-mysql-tests mysql -u syrx_user -pYourStrong!Passw0rd syrx -e "SELECT COUNT(*) FROM poco;"`

## Stopping the Test Database

```bash
# Stop and remove containers
docker-compose down

# Stop, remove containers, and delete volumes (fresh start)
docker-compose down -v

# Remove all associated images (complete cleanup)
docker-compose down -v --rmi all
```

## Performance Considerations

- Container uses persistent volumes to maintain data between restarts
- Official MySQL 8.0 base image for compatibility
- Health checks ensure database readiness before tests run
- Connection pooling handled by MySqlConnector driver

## Security Notes

- Default password should be changed for production-like environments
- Container runs on localhost only by default
- Database user has comprehensive privileges for testing purposes
- SYSTEM_USER privilege granted for MySQL 8.0 compatibility

## Compatibility

This setup is compatible with:
- MySQL 8.0
- .NET 8.0+
- Docker Engine 20.10+
- Docker Compose 2.0+
- Windows, macOS, and Linux development environments