# LibraryManagement

# Coding Problem:
REST API service - library.

Build me a backend for Library application. Library can have books and clients. Clients can take and return books. There can be role of Librarian who can add or remove books from library.

## Solution has below features

### Authentication:
- **JWT-based authentication**: .NET based JWT authentication
- **Role-based authorization**: Supports two roles - Librarian and Client
- **User creation**: 
  - Pre-created users are available in `SeedData.cs` file
  - Assumption: Librarian role users will be created from backend
  - Authentication APIs support creation of only Client users
  - Note: We can consider using .NET's `IdentityDbContext` based identity schema to store username and password, but considering time constraints, this has been excluded

### Database
- Uses .NET provided in-memory database to avoid complexity

### Models:
- **User** - with roles (Librarian/Client)
- **Book** - with title, author, ISBN, and copy management
- **BorrowRecord** - tracks book borrowing and returns

## API Endpoints:

### Auth:
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register new client

### Books (Librarians only for CUD operations):
- `GET /api/books` - Get all books
- `GET /api/books/{id}` - Get specific book
- `POST /api/books` - Add new book (Librarian only)
- `PUT /api/books/{id}` - Update book (Librarian only)
- `DELETE /api/books/{id}` - Remove book (Librarian only)

### Borrow Operations:
- `POST /api/borrow/borrow/{bookId}` - Borrow a book
- `POST /api/borrow/return/{borrowId}` - Return a book
- `GET /api/borrow/my-borrows` - Get user's borrow history
- `GET /api/borrow/all` - Get all borrows (Librarian only)

## Default Users:
- **Librarian**: username `librarian`, password `admin123`
- **Client**: username `client1`, password `pass123`

## Unit Test Cases
Added unit test cases specific to service classes. Not covered everything because of time contraints.

## How to Start the Application

1. **Compile source code**: Compile the `LibraryManagement` application. This should install necessary NuGet dependencies
2. **Start application**: Press F5 and start the application using "Http" profile
3. **Access Swagger**: You should see the Swagger page at `http://localhost:5189/swagger/index.html`
4. **Login**: 
   - Navigate to `/api/auth/login` endpoint
   - Enter credentials as shown in the **Default Users** section
   - Sample data can be found in `SeedData.cs` file
5. **Fetch books**: Use the `/api/Books` endpoint to fetch all books
6. **Explore endpoints**: Use the retrieved records to test other endpoints
7. **Access protected endpoints**:
   - To fetch all borrowed records, use librarian credentials
   - Fetch the token from login response
   - Click "Authorize" button (top right of Swagger page)
   - Set the Bearer token in the authorization section
8. **Perform borrow operations**: You can perform Borrow operations using the same user or any user with "Client" role