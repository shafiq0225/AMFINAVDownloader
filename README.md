# AMFI NAV Downloader

A Windows Service that automatically downloads AMFI NAV (Net Asset Value) data daily from the official AMFI India website.

## Features

- ✅ Daily scheduled download at configurable time (default: 8:00 AM)
- ✅ Automatic weekend and market holiday detection
- ✅ SQL Server database storage with duplicate prevention
- ✅ Self-contained Windows Service deployment
- ✅ Comprehensive logging with Serilog
- ✅ NSE holiday API integration for dynamic holiday detection

## Architecture

- Clean Architecture with Domain, Application, Infrastructure, and Console layers
- Entity Framework Core with Code-First approach
- Quartz.NET for job scheduling
- SQL Server for data persistence

## Prerequisites

- .NET 8.0 SDK
- SQL Server (Express or higher)
- Windows OS (for Windows Service deployment)

## Configuration

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AMFINAVDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AppSettings": {
    "NavSourceUrl": "https://portal.amfiindia.com/spages/NAVAll.txt",
    "ScheduleTime": "08:00:00",
    "RetryCount": 3,
    "RetryDelaySeconds": 10
  }
}
