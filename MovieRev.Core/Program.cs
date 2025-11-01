using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions; // Используем новое пространство имен для расширений
using MovieRev.Core.Settings; 
using MovieRev.Core.Services; 
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Настройка сервисов ---

builder.Services.AddSwaggerDocumentation(); // Используем метод расширения для Swagger

builder.Services.AddCoreServices(builder.Configuration); // Используем метод расширения для основных сервисов

// Custom VSA Extensions (теперь в Extensions)
builder.Services.AddEndPoints();

var app = builder.Build();

// --- 2. Конфигурация HTTP-пайплайна ---

app.UseSwaggerMiddleware(); // Используем метод расширения для Swagger UI
    
app.AddMiddleware(); // Используем метод расширения для middleware аутентификации/авторизации

app.MapEndPoints(); 

app.Run();