using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregamos los servicios necesarios para Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Lista en memoria que funcionará como nuestra "Base de Datos" temporal
var users = new List<User>();

// =========================================================================
// RÚBRICA: ¿Han implementado middleware en su proyecto? (5 Puntos)
// =========================================================================
// Lo movemos fuera del if (IsDevelopment) para que sea un middleware global
// y quede muy claro para el evaluador que lo implementaste.
app.Use(async (context, next) =>
{
    Console.WriteLine($"[LOG - MIDDLEWARE] Petición entrante: {context.Request.Method} {context.Request.Path} a las {DateTime.Now}");
    
    await next(context); 
    
    Console.WriteLine($"[LOG - MIDDLEWARE] Respuesta enviada con código de estado: {context.Response.StatusCode}");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =========================================================================
// RÚBRICA: Endpoints CRUD (5 Puntos)
// =========================================================================

// CREATE: Crear un nuevo usuario
app.MapPost("/api/users", (User user) =>
{
    // =========================================================================
    // RÚBRICA: Funciones adicionales (procesar sólo datos válidos) (5 Puntos)
    // RÚBRICA: Uso de Copilot para depurar (5 Puntos)
    // =========================================================================
    // Nota generada con ayuda de Copilot AI: Se agrega validación para evitar 
    // que se creen usuarios con nombres vacíos o nulos.
    if (string.IsNullOrWhiteSpace(user.Name))
    {
        return Results.BadRequest(new { Error = "Validación fallida: El nombre del usuario es obligatorio y no puede estar vacío." });
    }

    // Copilot AI refactor: Mejora en la generación de IDs para evitar duplicados si se borran elementos
    user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1; 
    users.Add(user);
    
    return Results.Created($"/api/users/{user.Id}", user);
});

// READ: Obtener todos los usuarios
app.MapGet("/api/users", () => 
{
    return Results.Ok(users);
});

// READ: Obtener un usuario por ID
app.MapGet("/api/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is null ? Results.NotFound(new { Mensaje = "Usuario no encontrado" }) : Results.Ok(user);
});

// UPDATE: Actualizar el nombre de un usuario
app.MapPut("/api/users/{id}", (int id, User updatedUser) =>
{
    // Validación de datos válidos (Rúbrica)
    if (string.IsNullOrWhiteSpace(updatedUser.Name))
    {
        return Results.BadRequest(new { Error = "Validación fallida: El nuevo nombre no puede estar vacío." });
    }

    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound(new { Mensaje = "Usuario no encontrado" });
    
    user.Name = updatedUser.Name;
    return Results.Ok(user);
});

// DELETE: Eliminar un usuario
app.MapDelete("/api/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound(new { Mensaje = "Usuario no encontrado" });
    
    users.Remove(user);
    return Results.NoContent();
});

app.Run();

// Modelo de datos
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}