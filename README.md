# LibraryManagement

# Coding Problem:
REST API service - library.
Build me a backend for Library application. Library can have books and clients. Clients can take and return  books. There can be role of Librarian who can add or remove boxes from library.


##Solution has below features

**Authentication:**
- JWT-based authentication : .Net based JWT authentication.
- Role-based authorization : It supports two roles : Librarian and Client
- We have already created different users in SeedData.cs files. Assumption is Librarian role users will be created from backend and Authentication apis will support creation of only one type of user (i.e. Client)
- We can think about using .net's IdentityDbContext based identity schema to store the username and passord but considering time limit we have ignored that.

**Database**
To avoid complexity, We have used .net provided in-memory database. 

**Models:**
- **User** - with roles (Librarian/Client)
- **Book** - with title, author, ISBN, and copy management
- **BorrowRecord** - tracks book borrowing and returns

**API Endpoints:**

**Auth:**
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Register new client

**Books (Librarians only for CUD operations):**
- `GET /api/books` - Get all books
- `GET /api/books/{id}` - Get specific book
- `POST /api/books` - Add new book (Librarian only)
- `PUT /api/books/{id}` - Update book (Librarian only)
- `DELETE /api/books/{id}` - Remove book (Librarian only)

**Borrow Operations:**
- `POST /api/borrow/borrow/{bookId}` - Borrow a book
- `POST /api/borrow/return/{borrowId}` - Return a book
- `GET /api/borrow/my-borrows` - Get user's borrow history
- `GET /api/borrow/all` - Get all borrows (Librarian only)

**Default Users:**
- Librarian: username `librarian`, password `admin123`
- Client: username `client1`, password `pass123`

**Unit test cases**
Added few unit test cases specific to service classes

**How to start the application**
1. Compile source code (LibraryManagement) application. This should install necessary nuget dependencies.
2. Pree F5 and start the the application using "Http"
3. You should see Swagger page upon start on the address "http://localhost:5189/swagger/index.html"
4. Head towards api/auth/login. Enter credentials as shown in **Default Users** section. You can see sample data in SeedData.cs file.
5. To fetch all the books use "/api/Books" endpoints.
6. Using these records we can play with other endpoints.
7. To fetch all the Borrowed records, You should use library credential, fetch the token and set the same in "Authorize" section of swagger page (Top right). This will set "Bearer" token for the context.
8.  You can perform "Borrow" operation using the same user or user with role "Client".
