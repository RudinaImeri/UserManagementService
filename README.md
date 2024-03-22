# User Management API

## Getting Started

After cloning the repository, you'll need to set up your database connection and initialize your database. Here's how:

1. **Configure Database Connection:** Open `appsettings.json` and navigate to the `ConnectionStrings` section. Add your database credentials here to establish the connection.
   
2. **Initialize Database:** Open your SQL Server and execute the following command to create the UserManagement database:
  ```sql
   CREATE DATABASE UserManagement;
```
   
## Using the API

The User Management API provides functionality for registering and managing users. Here's how to use it:

1. **Register:** Start by registering a new user using the Register API. This step is necessary to create your user credentials.

2. **Login:** After registering, you can login using the Login API. During the login process, a token will be generated. The API will return this token along with the user id. This token is essential for further interactions with the API.

3. **Manage Profile:** With the token, you can now perform actions such as getting the profile information, updating the profile, and deleting it.

## Configuration for Frontend Usage

If you intend to use this API with a frontend application, you'll need to make an additional configuration:

- **Set JWT Audience:** In `appsettings.json`, navigate to the `Jwt` section and change the `audience` value to your frontend project name. This step ensures that the token is correctly recognized when making requests from your frontend.

## Logs

For the User Management API:

- **Accessing Logs:** You can find the log files under `UserManagement.API/Logs`. These logs can provide valuable insights into the API's operation and help troubleshoot any issues.


