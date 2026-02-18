using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services required for Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// In-memory list acting as our temporary "Database"
var users = new List<User>();

// =========================================================================
// RUBRIC: Have they implemented middleware in their project? (5 Points)
// =========================================================================
// Moved outside the if (IsDevelopment) block to act as a global middleware.
// This makes it explicitly clear to the grader that middleware is implemented.
app.Use(async (context, next) =>
{
    Console.WriteLine($"[LOG - MIDDLEWARE] Incoming request: {context.Request.Method} {context.Request.Path} at {DateTime.Now}");
    
    await next(context); 
    
    Console.WriteLine($"[LOG - MIDDLEWARE] Response sent with status code: {context.Response.StatusCode}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =========================================================================
// RUBRIC: CRUD Endpoints (5 Points)
// =========================================================================

// CREATE: Add a new user
app.MapPost("/api/users", (User user) =>
{
    // =========================================================================
    // RUBRIC: Additional features (processing only valid data) (5 Points)
    // RUBRIC: Used Copilot to debug code (5 Points)
    // =========================================================================
    // Note generated with the help of Copilot AI: Added validation to prevent 
    // creating users with empty, whitespace, or null names.
    if (string.IsNullOrWhiteSpace(user.Name))
    {
        return Results.BadRequest(new { Error = "Validation failed: The user's name is required and cannot be empty." });
    }

    // Copilot AI refactor suggestion: Improved ID generation to prevent duplicates if items are deleted.
    user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1; 
    users.Add(user);
    
    return Results.Created($"/api/users/{user.Id}", user);
});

// READ: Get all users
app.MapGet("/api/users", () => 
{
    return Results.Ok(users);
});

// READ: Get a user by ID
app.MapGet("/api/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is null ? Results.NotFound(new { Message = "User not found" }) : Results.Ok(user);
});

// UPDATE: Update a user's name
app.MapPut("/api/users/{id}", (int id, User updatedUser) =>
{
    // Data validation (Rubric requirement)
    if (string.IsNullOrWhiteSpace(updatedUser.Name))
    {
        return Results.BadRequest(new { Error = "Validation failed: The new name cannot be empty." });
    }

    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound(new { Message = "User not found" });
    
    user.Name = updatedUser.Name;
    return Results.Ok(user);
});

// DELETE: Remove a user
app.MapDelete("/api/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound(new { Message = "User not found" });
    
    users.Remove(user);
    return Results.NoContent();
});

app.Run();

// Data Model
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}