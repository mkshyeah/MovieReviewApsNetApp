using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieRes.Core.Data;
using MovieRes.Core.EndPoints;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Убедитесь, что этот using присутствует
using System.Reflection; // Добавлен, если нужен для AddEndPoints/AddValidatorsFromAssembly

var builder = WebApplication.CreateBuilder(args);

// --- 1. Настройка сервисов ---

// ИСПОЛЬЗУЕМ SWASHBUCKLE ВМЕСТО AddOpenApi()
builder.Services.AddEndpointsApiExplorer(); // Необходим для обнаружения Minimal API Endpoints
builder.Services.AddSwaggerGen();           // Добавляет генератор спецификации

// Настройка контекста БД
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// УЛУЧШЕНИЕ 1: Добавляем сервисы Аутентификации и Авторизации.
// AddAuthentication добавляется неявно через AddIdentity, но AddAuthorization нужно явно
builder.Services.AddAuthentication();


// Custom VSA Extensions
builder.Services.AddEndPoints();

// FluentValidatio
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly); 

var app = builder.Build();

// --- 2. Конфигурация HTTP-пайплайна ---

if (app.Environment.IsDevelopment())
{
    // ИСПОЛЬЗУЕМ SWASHBUCKLE UI ВМЕСТО MapOpenApi()
    app.UseSwagger();           // Создает JSON-файл спецификации
    app.UseSwaggerUI();         // Добавляет пользовательский интерфейс Swagger UI
    
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// УЛУЧШЕНИЕ 2: Middleware для аутентификации и авторизации
// Эти два middleware КРИТИЧНЫ для работы ClaimsPrincipal user в хэндлерах.
// Они должны стоять ПЕРЕД MapEndPoints().
app.UseAuthentication();
app.UseAuthorization();

app.MapEndPoints(); 

app.Run();