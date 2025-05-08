using Microsoft.EntityFrameworkCore;
using Tango.RBAC.RbacServicePackage.Data;
using Tango.RBAC.RbacServicePackage.Interfaces;
using Tango.RBAC.TestData;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using Tango.RBAC.RbacServicePackage.Models;
using Microsoft.OpenApi.Models;
using Tango.RBAC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
builder.Services.AddDbContext<RbacDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();


var app = builder.Build();

// Use Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "RBAC API V1");
});

// Apply migrations and seed test data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RbacDbContext>();
    await db.Database.MigrateAsync(); // Ensures tables are created
    await RbacTestData.SeedTestDataAsync(db); // Optional: Seed initial data
}

// Default test endpoint
app.MapGet("/", () => "Tango RBAC Demo App running.")
    .WithName("Default")
    .WithSummary("Default health check endpoint")
    .WithDescription("Returns a basic message to confirm the app is running.");

app.MapPost("/assign-role", async (IAuthorizationService service, int userId, int roleId, string user) =>
{
    await service.AssignRoleToUserAsync(userId, roleId, user);
    return Results.Ok("Role assigned to user.");
})
.WithName("AssignRoleToUser")
.WithSummary("Assigns a role to a user")
.WithDescription("Assigns the specified role to the given user.");

app.MapPost("/assign-permission", async (IAuthorizationService service, int roleId, int permissionId, string user) =>
{
    await service.AssignPermissionToRoleAsync(roleId, permissionId, user);
    return Results.Ok("Permission assigned to role.");
})
.WithName("AssignPermissionToRole")
.WithSummary("Assigns a permission to a role")
.WithDescription("Assigns the specified permission to the given role.");

app.MapGet("/has-permission", async (IAuthorizationService service, int userId, int areaTypeId, int permissionTypeId) =>
{
    var result = await service.HasPermissionAsync(userId, areaTypeId, permissionTypeId);
    return Results.Ok(result);
})
.WithName("CheckUserPermission")
.WithSummary("Checks if a user has a specific permission")
.WithDescription("Verifies whether the user is in a role that has the specified permission.");

app.MapPost("/add-user", async (IAuthorizationService service, User user) =>
{
    var result = await service.AddUserAsync(user);
    return Results.Created($"/users/{result.UserId}", result);
})
.WithName("AddUser")
.WithSummary("Adds a new user")
.WithDescription("Creates and returns a new user.");

app.MapPost("/add-role", async (IAuthorizationService service, Role role) =>
{
    var result = await service.AddRoleAsync(role);
    return Results.Created($"/roles/{result.RoleId}", result);
})
.WithName("AddRole")
.WithSummary("Adds a new role")
.WithDescription("Creates and returns a new role.");

app.MapPost("/add-permission", async (IAuthorizationService service, Permission permission) =>
{
    var result = await service.AddPermissionAsync(permission);
    return Results.Created($"/permissions/{result.PermissionId}", result);
})
.WithName("AddPermission")
.WithSummary("Adds a new permission")
.WithDescription("Creates and returns a new permission.");

app.MapGet("/user/{id}", async (IAuthorizationService service, int id) =>
{
    var user = await service.GetUserByIdAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
})
.WithName("GetUserById")
.WithSummary("Gets a user by ID")
.WithDescription("Fetches and returns a user with the specified ID.");

app.MapGet("/role/{id}", async (IAuthorizationService service, int id) =>
{
    var role = await service.GetRoleByIdAsync(id);
    return role is not null ? Results.Ok(role) : Results.NotFound();
})
.WithName("GetRoleById")
.WithSummary("Gets a role by ID")
.WithDescription("Fetches and returns a role with the specified ID.");

app.MapGet("/permission/{id}", async (IAuthorizationService service, int id) =>
{
    var permission = await service.GetPermissionByIdAsync(id);
    return permission is not null ? Results.Ok(permission) : Results.NotFound();
})
.WithName("GetPermissionById")
.WithSummary("Gets a permission by ID")
.WithDescription("Fetches and returns a permission with the specified ID.");

app.MapDelete("/user/{id}", async (IAuthorizationService service, int id) =>
{
    await service.DeleteUserAsync(id);
    return Results.NoContent();
})
.WithName("DeleteUser")
.WithSummary("Deletes a user")
.WithDescription("Removes the user with the specified ID from the system.");

app.MapDelete("/role/{id}", async (IAuthorizationService service, int id) =>
{
    await service.DeleteRoleAsync(id);
    return Results.NoContent();
})
.WithName("DeleteRole")
.WithSummary("Deletes a role")
.WithDescription("Removes the role with the specified ID from the system.");

app.MapDelete("/permission/{id}", async (IAuthorizationService service, int id) =>
{
    await service.DeletePermissionAsync(id);
    return Results.NoContent();
})
.WithName("DeletePermission")
.WithSummary("Deletes a permission")
.WithDescription("Removes the permission with the specified ID from the system.");


app.Run();