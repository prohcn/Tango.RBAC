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

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();


// In-memory DB for demo purposes
builder.Services.AddDbContext<RbacDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// Register services
builder.Services.AddScoped<IAuthorizationService, Tango.RBAC.Services.AuthorizationService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Seed test data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RbacDbContext>();
    await db.Database.MigrateAsync(); // apply schema
    await RbacTestData.SeedTestDataAsync(db);   // seed test data
}

// Default test endpoint
app.MapGet("/", () => "Tango RBAC Demo App with demo data.");

// Get user by ID
app.MapGet("/users/{id}", async (int id, IAuthorizationService service) =>
{
    var user = await service.GetUserByIdAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
})
.WithName("GetUser")
.WithSummary("Gets a user by id")
.WithDescription("Gets existing user information by id");

// Get role by ID
app.MapGet("/roles/{id}", async (int id, IAuthorizationService service) =>
{
    var role = await service.GetRoleByIdAsync(id);
    return role is not null ? Results.Ok(role) : Results.NotFound();
})
.WithName("GetRole")
.WithSummary("Gets a role by id")
.WithDescription("Gets existing role information by id");

// Get permission by ID
app.MapGet("/permissions/{id}", async (int id, IAuthorizationService service) =>
{
    var perm = await service.GetPermissionByIdAsync(id);
    return perm is not null ? Results.Ok(perm) : Results.NotFound();
})
.WithName("GetPermission")
.WithSummary("Gets a permission by id")
.WithDescription("Gets existing permission information by id");

// Assign role to user
app.MapPost("/users/{userId}/roles/{roleId}", async (int userId, int roleId, IAuthorizationService service) =>
{
    await service.AssignRoleToUserAsync(userId, roleId);
    return Results.Ok();
})
.WithName("PostRoleToUser")
.WithSummary("Assigns Role to a User");

// Assign permission to role
app.MapPost("/roles/{roleId}/permissions/{permissionId}", async (int roleId, int permissionId, IAuthorizationService service) =>
{
    await service.AssignPermissionToRoleAsync(roleId, permissionId);
    return Results.Ok();
})
.WithName("PostPermissionToRole")
.WithSummary("Assigns Permission to a Role");

// Assign permission to user (override)
app.MapPost("/users/{userId}/permissions/{permissionId}", async (int userId, int permissionId, string overrideMode, IAuthorizationService service) =>
{
    await service.AssignPermissionToUserAsync(userId, permissionId, overrideMode);
    return Results.Ok();
})
.WithName("PostPermissionToUser")
.WithSummary("Assigns Permission to a User");
// === IAuthorizationService Endpoints ===

app.MapGet("/check-permission", async (int userId, string area, string name, IAuthorizationService service) =>
{
    bool hasPermission = await service.HasPermissionAsync(userId, area, name);
    return Results.Ok(new { userId, area, name, hasPermission });
})
.WithName("UserPermissionCheck")
.WithSummary("Checks a users permission for a specific area and name");

app.MapPost("/users", async (User user, IAuthorizationService service) =>
{
    var result = await service.AddUserAsync(user);
    return Results.Created($"/users/{result.UserId}", result);
})
.WithName("AddUser")
.WithSummary("Adds a new user");

app.MapPost("/roles", async (Role role, IAuthorizationService service) =>
{
    var result = await service.AddRoleAsync(role);
    return Results.Created($"/roles/{result.RoleId}", result);
})
.WithName("AddRole")
.WithSummary("Adds a new role");

app.MapPost("/permissions", async (Permission permission, IAuthorizationService service) =>
{
    var result = await service.AddPermissionAsync(permission);
    return Results.Created($"/permissions/{result.PermissionId}", result);
})
.WithName("AddPermission")
.WithSummary("Adds a new permission");

app.MapPut("/users", async (User user, IAuthorizationService service) =>
{
    var result = await service.UpdateUserAsync(user);
    return Results.Ok(result);
})
.WithName("UpdateUser")
.WithSummary("Updates a user");

app.MapPut("/roles", async (Role role, IAuthorizationService service) =>
{
    var result = await service.UpdateRoleAsync(role);
    return Results.Ok(result);
})
.WithName("UpdateRole")
.WithSummary("Updates a role");

app.MapPut("/permissions", async (Permission permission, IAuthorizationService service) =>
{
    var result = await service.UpdatePermissionAsync(permission);
    return Results.Ok(result);
})
.WithName("UpdatePermission")
.WithSummary("Updates a permission");

app.MapDelete("/users/{userId}", async (int userId, IAuthorizationService service) =>
{
    await service.DeleteUserAsync(userId);
    return Results.NoContent();
})
.WithName("DeleteUser")
.WithSummary("Deletes a user");

app.MapDelete("/roles/{roleId}", async (int roleId, IAuthorizationService service) =>
{
    await service.DeleteRoleAsync(roleId);
    return Results.NoContent();
})
.WithName("DeleteRole")
.WithSummary("Deletes a role");

app.MapDelete("/permissions/{permissionId}", async (int permissionId, IAuthorizationService service) =>
{
    await service.DeletePermissionAsync(permissionId);
    return Results.NoContent();
})
.WithName("DeletePermission")
.WithSummary("Deletes a permission");

app.MapPost("/users/{userId}/roles/{roleId}/assign", async (int userId, int roleId, DateTime? effectiveFrom, DateTime? effectiveThrough, IAuthorizationService service) =>
{
    await service.AssignRoleToUserAsync(userId, roleId, effectiveFrom, effectiveThrough);
    return Results.Ok();
})
.WithName("AssignRoleToUser")
.WithSummary("Assigns role to a user");

app.MapPost("/roles/{roleId}/permissions/{permissionId}/assign", async (int roleId, int permissionId, IAuthorizationService service) =>
{
    await service.AssignPermissionToRoleAsync(roleId, permissionId);
    return Results.Ok();
})
.WithName("AssignPermissionToRole")
.WithSummary("Assigns a permission to a role");

app.MapPost("/users/{userId}/permissions/{permissionId}/grant", async (int userId, int permissionId, IAuthorizationService service) =>
{
    await service.GrantPermissionToUserAsync(userId, permissionId);
    return Results.Ok();
})
.WithName("GrantPermissionToUser")
.WithSummary("Assigns a permission to a user");

app.MapPost("/users/{userId}/permissions/{permissionId}/deny", async (int userId, int permissionId, IAuthorizationService service) =>
{
    await service.DenyPermissionToUserAsync(userId, permissionId);
    return Results.Ok();
})
.WithName("DenyPermissionToUser")
.WithSummary("Deny's a permission to a user");

app.MapDelete("/users/{userId}/permissions/{permissionId}/delete", async (int userId, int permissionId, IAuthorizationService service) =>
{
    await service.RemoveUserPermissionOverrideAsync(userId, permissionId);
    return Results.Ok();
})
.WithName("DeleteExistingPermissionToUser")
.WithSummary("Deletes an existing permission to a user");

app.Run();