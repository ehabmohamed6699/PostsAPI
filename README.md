# PostsAPI
An API for creating and managing posts, tags, and comments with authentication and role-based authorization. Built with ASP.NET Core (.NET 10), EF Core, SQL Server, JWT.

## Features
- **Posts and Comments:** Ability to create and manipulate posts with comments.
- **Searching, Sorting, and Pagination:** Optimized data querying with searching, sorting, and pagination query parameters.
- **Authentication & Authorization:** JWT-based authentication and role-based authorization.
- **Tags system:** Attachable tags with M-N relationship and only-admin policy for adding new tags.
- **Ownership Rules:** Ownership of system entities (Posts and Comments) with authorized endpoints.
- **Atomic Likes System:** Valid likes system that uses atomic sql update to ensure data consistency after liking.
- **Efficient Logging & Monitoring:** Registered middlewares for logging requests time, results, and traceable errors.
- **Consistent Error Response:** Clear error responses for clients with trace identifiers for server errors.

## Run it
To be able to run this project you need first to have docker installed on your local machine

1- Include a .env file with the following content (ensure to replace placeholders with your actual values):
```.env
ConnectionStrings__DefaultConnection=<your db connection string>
JwtSettings__Key=<a long random string>
JwtSettings__Issuer=<issuer name>
JwtSettings__Audience=<audience name>
JwtSettings__ExpiryMinutes=<token lifetime in minutes>
```
2- Run the following commands:
```bash
docker build -t postsapi .
docker run --rm -p 8080:8080 --env-file .env --name api postsapi
```
3- Navigate to http://localhost:8080/health and ensure it says "Healthy".

4- Ensure the health request returned 200 by checking server logs:
```bash
docker logs api
```
To run the full API locally, follow these steps:

1- Make sure to add the database connection string and the JWT key to user secrets:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your string>"
dotnet user-secrets set "JwtSettings:Key" "<a long random string>"
```
2- Migrate to the database
```bash
dotnet ef database update
```
2- Run the project:
```bash
dotnet run
```
NOTE: make sure you promote a user to admin in order to be able to access admin-only resources

## Tests
To be able to run the testing project to test the server app follow these steps:

1- Set a database connection string for the current environment
```bash
$env:POSTSAPI_TEST_DB = "your-db-connection-string"
```
The template for a SQL Server connection string is like this:
```txt
Server=server-url-for-db;Database=PostsApiTestDb;User Id=sa;Password=your-db-conn-password;TrustServerCertificate=true;
```

2- Run the testing project: 
```bash
dotnet test
```

## API overview
| Method | Route | Auth | What it does |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | none | registers new user using email and password |
| POST | `/api/auth/login` | none | logs in users with credentials and returns JWT token |
| GET | `/api/posts` | none | returns paginated posts (accepts search, sort, and page query parameters) |
| GET | `/api/posts/{id}` | none | returns a post of specific ID |
| GET | `/api/tags` | none | returns all tags in the system |
| GET | `/api/tags/{id}` | none | returns a tag of specific ID |
| POST | `/api/posts` | authorized | creates a new post |
| POST | `/api/posts/{id}/comments` | authorized | adds a new comment for a post of specific ID |
| POST | `/api/posts/{id}/like` | authorized | adds a like for a post of specific ID |
| PUT | `/api/posts/{id}` | authorized | updates a post of specific ID |
| PUT | `/api/posts/{id}/tags/{tagId}` | authorized | attaches a tag to a post of specific ID |
| DELETE | `/api/posts/{id}` | authorized | deletes a post of specific ID |
| POST | `/api/tags` | admin only | creates a new tag |

## Architecture
See [ARCHITECTURE.md](ARCHITECTURE.md)