# Contoso Inventory API Reference

This reference covers the endpoints in `AuthController` and `CategoriesController`.
Examples use `http://localhost:5240`; replace it with the deployed API origin.

## General behavior

- Authentication uses the `.ContosoInventory.Auth` HTTP-only cookie. Send the cookie
  on subsequent requests; the login response sets it. For browser clients, include
  credentials when making cross-origin requests.
- All category endpoints require an authenticated user. Creating, updating, deleting,
  and toggling a category additionally require the `Admin` role.
- The API applies model validation automatically. Invalid request bodies return
  `400 Bad Request`.
- A global per-IP rate limiter allows 100 requests per minute and queues up to 10
  additional requests; excess requests receive `429 Too Many Requests`.
- Authentication failures return `401 Unauthorized`; an authenticated user without
  the required role receives `403 Forbidden`.
- An unexpected server-side failure may result in `500 Internal Server Error`; the
  controllers do not define a common response body for these failures.

## Authentication

### Log in

- **Method and route:** `POST /api/Auth/login`
- **Purpose:** Verifies credentials and establishes the authentication cookie.
- **Authentication:** Anonymous.
- **Inputs:** JSON request body; no route or query parameters.
- **Request body:**
  - `email` (string, required, valid email address)
  - `password` (string, required)
- **Responses:**
  - `200 OK` — Credentials are valid; returns the user profile and sets the auth cookie.
  - `400 Bad Request` — The request body is missing or fails DTO validation.
  - `401 Unauthorized` — Credentials are invalid, the account was not found after sign-in,
    or the account is temporarily locked.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "alex@example.com",
  "password": "ExamplePassword123!"
}
```

Response (`200 OK`):

```json
{
  "email": "alex@example.com",
  "displayName": "Alex Example",
  "role": "Admin"
}
```

The response also includes a `Set-Cookie` header for `.ContosoInventory.Auth`.
The cookie value is issued by the server and is not part of the JSON response.

### Log out

- **Method and route:** `POST /api/Auth/logout`
- **Purpose:** Signs out the current user and clears the authentication cookie.
- **Authentication:** Required.
- **Inputs:** No route, query, or body parameters.
- **Responses:**
  - `200 OK` — The user was signed out.
  - `401 Unauthorized` — No valid authentication cookie was supplied.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
POST /api/Auth/logout
Cookie: .ContosoInventory.Auth=<authentication-cookie>
```

Response (`200 OK`):

```json
{
  "message": "Logged out successfully."
}
```

### Get the current user

- **Method and route:** `GET /api/Auth/me`
- **Purpose:** Returns the profile for the currently authenticated user.
- **Authentication:** Required.
- **Inputs:** No route, query, or body parameters.
- **Responses:**
  - `200 OK` — Returns the current user's profile.
  - `401 Unauthorized` — The request is unauthenticated or the user could not be resolved.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
GET /api/Auth/me
Cookie: .ContosoInventory.Auth=<authentication-cookie>
```

Response (`200 OK`):

```json
{
  "email": "alex@example.com",
  "displayName": "Alex Example",
  "role": "Admin"
}
```

## Categories

Category responses use this shape:

```json
{
  "id": 12,
  "name": "Laptops",
  "description": "Portable computers",
  "displayOrder": 1,
  "isActive": true,
  "createdDate": "2026-10-23T12:00:00Z",
  "lastModifiedDate": "2026-10-23T12:00:00Z"
}
```

### List categories

- **Method and route:** `GET /api/Categories`
- **Purpose:** Returns all categories, ordered by display order.
- **Authentication:** Required; any authenticated role.
- **Inputs:** No route, query, or body parameters.
- **Responses:**
  - `200 OK` — Returns an array of categories; an empty array is returned when none exist.
  - `401 Unauthorized` — No valid authentication cookie was supplied.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
GET /api/Categories
Cookie: .ContosoInventory.Auth=<authentication-cookie>
```

Response (`200 OK`):

```json
[
  {
    "id": 12,
    "name": "Laptops",
    "description": "Portable computers",
    "displayOrder": 1,
    "isActive": true,
    "createdDate": "2026-10-23T12:00:00Z",
    "lastModifiedDate": "2026-10-23T12:00:00Z"
  }
]
```

### Get a category

- **Method and route:** `GET /api/Categories/{id}`
- **Purpose:** Returns a category by its integer identifier.
- **Authentication:** Required; any authenticated role.
- **Inputs:** `id` (integer, route parameter). No query parameters or request body.
- **Responses:**
  - `200 OK` — Returns the matching category.
  - `400 Bad Request` — The route identifier cannot be converted to an integer.
  - `401 Unauthorized` — No valid authentication cookie was supplied.
  - `404 Not Found` — No category exists with that identifier.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
GET /api/Categories/12
Cookie: .ContosoInventory.Auth=<authentication-cookie>
```

Response (`200 OK`):

```json
{
  "id": 12,
  "name": "Laptops",
  "description": "Portable computers",
  "displayOrder": 1,
  "isActive": true,
  "createdDate": "2026-10-23T12:00:00Z",
  "lastModifiedDate": "2026-10-23T12:00:00Z"
}
```

### Create a category

- **Method and route:** `POST /api/Categories`
- **Purpose:** Creates a category. New categories are active by default.
- **Authentication:** Required; the user must have the `Admin` role.
- **Inputs:** JSON request body; no route or query parameters.
- **Request body:**
  - `name` (string, required, 2–100 characters)
  - `description` (string, required, up to 500 characters)
  - `displayOrder` (integer, 1–1000)
- **Responses:**
  - `201 Created` — Returns the created category and a `Location` header pointing to it.
  - `400 Bad Request` — The body fails validation or the name is already in use
    (duplicate names are compared without case sensitivity).
  - `401 Unauthorized` — No valid authentication cookie was supplied.
  - `403 Forbidden` — The authenticated user does not have the `Admin` role.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
POST /api/Categories
Content-Type: application/json
Cookie: .ContosoInventory.Auth=<authentication-cookie>

{
  "name": "Laptops",
  "description": "Portable computers",
  "displayOrder": 1
}
```

Response (`201 Created`):

```json
{
  "id": 12,
  "name": "Laptops",
  "description": "Portable computers",
  "displayOrder": 1,
  "isActive": true,
  "createdDate": "2026-10-23T12:00:00Z",
  "lastModifiedDate": "2026-10-23T12:00:00Z"
}
```

### Update a category

- **Method and route:** `PUT /api/Categories/{id}`
- **Purpose:** Replaces the editable fields and active status of a category.
- **Authentication:** Required; the user must have the `Admin` role.
- **Inputs:** `id` (integer, route parameter); JSON request body; no query parameters.
- **Request body:**
  - `name` (string, required, 2–100 characters)
  - `description` (string, required, up to 500 characters)
  - `displayOrder` (integer, 1–1000)
  - `isActive` (boolean)
- **Responses:**
  - `200 OK` — Returns the updated category.
  - `400 Bad Request` — The body fails validation or the name is already in use
    (duplicate names are compared without case sensitivity), or the route identifier
    cannot be converted to an integer.
  - `401 Unauthorized` — No valid authentication cookie was supplied.
  - `403 Forbidden` — The authenticated user does not have the `Admin` role.
  - `404 Not Found` — No category exists with that identifier.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
PUT /api/Categories/12
Content-Type: application/json
Cookie: .ContosoInventory.Auth=<authentication-cookie>

{
  "name": "Laptops",
  "description": "Portable computers",
  "displayOrder": 1,
  "isActive": true
}
```

Response (`200 OK`):

```json
{
  "id": 12,
  "name": "Laptops",
  "description": "Portable computers",
  "displayOrder": 1,
  "isActive": true,
  "createdDate": "2026-10-23T12:00:00Z",
  "lastModifiedDate": "2026-10-23T12:30:00Z"
}
```

### Delete a category

- **Method and route:** `DELETE /api/Categories/{id}`
- **Purpose:** Permanently deletes a category.
- **Authentication:** Required; the user must have the `Admin` role.
- **Inputs:** `id` (integer, route parameter). No query parameters or request body.
- **Responses:**
  - `204 No Content` — The category was deleted; the response has no body.
  - `401 Unauthorized` — No valid authentication cookie was supplied.
  - `403 Forbidden` — The authenticated user does not have the `Admin` role.
  - `400 Bad Request` — The route identifier cannot be converted to an integer.
  - `404 Not Found` — No category exists with that identifier.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
DELETE /api/Categories/12
Cookie: .ContosoInventory.Auth=<authentication-cookie>
```

Response (`204 No Content`): no response body.

### Toggle category active status

- **Method and route:** `POST /api/Categories/{id}/toggle-active`
- **Purpose:** Toggles a category's `isActive` value and updates its last-modified date.
- **Authentication:** Required; the user must have the `Admin` role.
- **Inputs:** `id` (integer, route parameter). No query parameters or request body.
- **Responses:**
  - `200 OK` — Returns the category with its new active status.
  - `401 Unauthorized` — No valid authentication cookie was supplied.
  - `403 Forbidden` — The authenticated user does not have the `Admin` role.
  - `400 Bad Request` — The route identifier cannot be converted to an integer.
  - `404 Not Found` — No category exists with that identifier.
  - `429 Too Many Requests` — The global rate limit was reached.
  - `500 Internal Server Error` — An unexpected server-side failure occurred.

Request:

```http
POST /api/Categories/12/toggle-active
Cookie: .ContosoInventory.Auth=<authentication-cookie>
```

Response (`200 OK`):

```json
{
  "id": 12,
  "name": "Laptops",
  "description": "Portable computers",
  "displayOrder": 1,
  "isActive": false,
  "createdDate": "2026-10-23T12:00:00Z",
  "lastModifiedDate": "2026-10-23T12:30:00Z"
}
```
