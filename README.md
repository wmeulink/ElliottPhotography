# Elliott Photography API

## Live Application

The full photography platform, including frontend and API, is live at:  
https://whittyelliott.com

---

## Overview

Elliott Photography API is a production-ready backend service built with ASP.NET Core to support a full-stack photography platform.

The API handles image uploads, server-side processing, file storage, and relational data management. It exposes structured endpoints consumed by a React frontend, providing a seamless full-stack experience.  

This project emphasizes real-world backend concerns, including efficient data access, file system integration, and scalable API design.

---

## Core Capabilities

### Image Upload and Processing
- Accepts multipart/form-data uploads
- Stores full-resolution images on the server
- Generates optimized thumbnails using ImageSharp
- Applies resizing and compression for performance optimization

### File Storage and Retrieval
- Organizes images into structured directories (full and thumbnail variants)
- Maps database paths to physical file locations at runtime
- Serves images directly via API endpoints with correct content types

### Relational Data Management
- Uses PostgreSQL with Entity Framework Core
- Supports category-based organization (one-to-many)
- Implements tag relationships (many-to-many)
- Dynamically creates and reuses tags to maintain data integrity

### API Design
- RESTful endpoint structure
- Filtering capabilities (e.g., by category)
- Resource-specific routes for full images and thumbnails
- Clean separation of concerns between controllers and data access

### Data Shaping
- Uses DTOs to control API responses
- Prevents over-fetching and circular references
- Returns frontend-optimized data structures

### Contact and Communication
- Persists contact form submissions to the database
- Integrates with a custom email service for notifications

---

## Tech Stack

- C#
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- ImageSharp (image processing)
- Custom email service

---

## Architecture

The application follows a layered architecture designed for maintainability and scalability:

- Controllers handle HTTP routing and request/response lifecycles
- DTOs define structured response contracts
- Entity Framework Core manages ORM mapping and database interaction
- Services encapsulate cross-cutting concerns such as email delivery

Asynchronous programming patterns (async/await) are used throughout to ensure efficient handling of I/O-bound operations.

---

## Project Structure

    /Controllers   # API endpoints (Landscapes, Portraits, Contact)
    /Models        # Entity models and relationships
    /DTOs          # Response shaping and data contracts
    /Data          # DbContext and database configuration
    /Services      # Business logic and integrations

---

## Getting Started

    git clone https://github.com/wmeulink/ElliottPhotography.git
    cd ElliottPhotography
    dotnet run

---

## Configuration

Update `appsettings.json` with your PostgreSQL connection string:

    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
      }
    }

---

## API Endpoints

### Landscapes

- GET /api/landscapes  
- GET /api/landscapes/{id}  
- GET /api/landscapes/{id}/full  
- GET /api/landscapes/{id}/thumb  
- GET /api/landscapes/category/{category}  
- POST /api/landscapes/upload  

### Portraits

- GET /api/portraits  
- GET /api/portraits/{id}  
- GET /api/portraits/{id}/full  
- GET /api/portraits/{id}/thumb  
- GET /api/portraits/category/{category}  
- POST /api/portraits/upload  

### Contact

- POST /api/contact  
- GET /api/contact  

---

## Related Repository

Frontend:  
https://github.com/wmeulink/my-photography-app
