# Dingeleihe: Backend and Frontend Development

## About the Project
This project implements a comprehensive system for managing and lending household items such as mixers and drills for the Linz City Library. It consists of a backend part that provides a REST API for data management and a frontend part that provides an Angular-based user interface for interacting with the backend.

### Backend
The backend is built on ASP.NET Core and uses Entity Framework to access a PostgreSQL database. It provides endpoints for managing users, items, and lending transactions. Emphasis is placed on correct validation and testing through unit tests.

### Frontend
The frontend is developed with Angular and offers various tabs for displaying and managing items, users, and loans. It uses modern Angular techniques and is connected to the backend through REST API endpoints.

## Features
### Backend
- **Data modeling** for users, items, and loans with validation and lazy loading.
- **REST API** provides endpoints for CRUD operations on items and users, as well as special endpoints for lending processes.

### Frontend
- **Components** for displaying individual items and users, with the ability to switch between display and edit modes.
- **Repositories** manage the state and interactions of multiple instances of items, users, and loans.
- **Routing and Backend Integration** to handle navigation and data fetching from the backend API.

