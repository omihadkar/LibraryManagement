## Features

**Models:**
- **User** - with roles (Librarian/Client)
- **Book** - with title, author, ISBN, and copy management
- **BorrowRecord** - tracks book borrowing and returns

**Authentication:**
- JWT-based authentication
- Role-based authorization (Librarian/Client)

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

The API uses an in-memory database for easy testing. For production, replace `UseInMemoryDatabase` with SQL Server or PostgreSQL. The API includes Swagger UI for testing at `/swagger`.