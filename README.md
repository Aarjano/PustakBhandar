# PustakBhandar E-Library Application

A full-stack E-Library application with .NET Core backend (PostgreSQL) and React frontend.

## Project Overview

PustakBhandar is a comprehensive E-Library system designed to manage book catalogs, user accounts, bookmarks, shopping carts, and orders. The application follows a microservices architecture with a clear separation between the backend API and frontend client.

### Backend Features
- RESTful API built with .NET Core
- PostgreSQL database
- JWT authentication
- Swagger API documentation
- Entity Framework Core for ORM

### Frontend Features
- React with functional components and hooks
- React Router for navigation
- Context API for state management
- Tailwind CSS for styling
- Responsive design

## Project Structure

- `FinalProject/` - .NET Core backend application
- `frontend/` - React frontend application

## Prerequisites

- .NET SDK 6.0 or higher
- Node.js (v16+) and npm
- PostgreSQL (v12+)

## Backend Setup

1. Configure PostgreSQL:
   - Create a database named "PustakBhandar"
   - Update the connection string in `appsettings.json` if needed

2. Navigate to the backend directory:
```
cd ElibraryApp-main/FinalProject
```

3. Install required packages:
```
dotnet restore
```

4. Apply database migrations:
```
dotnet ef database update
```

5. Run the application:
```
dotnet run
```

The API will be available at http://localhost:5000, and Swagger UI at http://localhost:5000/swagger.

## Frontend Setup

1. Navigate to the frontend directory:
```
cd ElibraryApp-main/frontend
```

2. Install dependencies:
```
npm install
```

3. Start the development server:
```
npm start
```

The frontend will be available at http://localhost:3000.

## Features

1. User Management
   - Registration
   - Authentication
   - Profile management

2. Book Catalog
   - Browse books with pagination
   - Search, sort, and filter books
   - View book details and reviews

3. User Features
   - Bookmark books
   - Add books to cart
   - Place and manage orders
   - Get discounts based on quantity and order history

4. Admin Features
   - Manage book catalog (CRUD)
   - Set discounts and sales
   - Manage orders
   - Write announcements

5. Staff Features
   - Process orders using claim codes

## API Documentation

The API documentation is available through Swagger UI at http://localhost:5000/swagger when the backend is running.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
