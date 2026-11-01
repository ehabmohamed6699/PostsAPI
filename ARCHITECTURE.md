# Architecture

## What this is
This is a basic posts web API built using ASP.NET Core and Web API.
The API involves simple authentication system, posts with comments and tags, authorization system and route guarding.

## Request flow
Request -> Kestrel -> timing middleware -> exception middleware -> HTTPS redirect ->
authentication -> authorization -> controller -> service -> EF Core -> SQL Server

## Layers
- **Controllers:** They map HTTP requests to action names to execute the appropriate action (they tell what to do not how).
- **Services:** They identify the formula for executing the action (the how), here the scoped context lives where it is used.
- **AppDbContext:** It's a connection between C# and the database where EF code is mapped to SQL commands and vice versa.

## Decisions and trade-offs
**Exception Handling vs Result Pattern**
In the services I had to choose between throwing exceptions when something wrong happens (e.g. a post is not found or forbidden action) and using Result Pattern where I define a response class or record that holds both data and error.
While exceptions offer ease of use, less code written, and separation of error handling (in my exception handling middleware), Result Pattern, although they require more code and easy to ignore (I once computed result and returned `NoContent()` anyway which passed all positive tests hiding negative tests under fake success), they offered more adaptability and reliability compared to exceptions which are slightly slower that returning values.

**Model Classes vs DTOs**
DTOs are short for Data Transfer Objects which are used to structure the shape of data on transit
Model Classes are primarly for structuring the tables in the database, using them in data transit might be problematic which might (1) return sensitive data to the client. (2) allow the client to edit uneditable data.
DTOs solve this with more advantages, they identify what data is received from the client, what data is sent to it, and allow for derived and calculated props.

**JWT Token Life Time**
The JWT is the token that holds authenticated client's necessary payload with a signature, it is an authentication method to ensure only authorized persons access allowed resources.
a JWT must not live long as it might be hijacked and used by another non owner, a short life time for the JWT is necessary here to decrease the damage to the least possible.
I choose to set life time as 30 minutes with no refresh token so that stolen tokens don't live for long, the trade off is that the user must log in again as there is no revocation.

### Likes use an atomic UPDATE
Decision: `ExecuteUpdateAsync(Likes = Likes + 1)` instead of load, increment, save.
Why: a test with 50 parallel likes lost updates with the naive version.
Trade-off: it bypasses EF's change tracking, so any logic hooked into SaveChanges wouldn't run (none exists today).

### IPostService has one implementation
One design principle says "Program to an interface, not implementation"
In this case, I choose adding more file for the interface and the service class so the controller depends on the interface and could be tested with fake service.
### Split queries
Split queries splits join queries to separate queries for each table.
When using Single Query, the join query can introduce some performance issue such as cartesian explosion when multiple includes are of the same level or column duplication for single include
This is avoided by separating the queries for each table, which adds more queries but solves other problems

## What I'd change for a team of 10
**[1] Caching:** such app in real life might hit higher read rates (browsing posts and comments).
**[2] Load Balancing:** one server dealing with all social media app requests will eventually collapse, load balancing is a must.
**[3] Logging and Monitoring:** monitoring system performance and logging necessary information in central logging system, ensuring system validation and verification with feedback and monitoring results.
**[4] Layered Architecture:** the current structure is monolith, meaning all the code is now in a single project (disregarding the testing project), which leaves us with single ponit of modification and contribution, this might lead to conflicts.
