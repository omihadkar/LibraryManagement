using LibraryManagement.Context;
using LibraryManagement.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT token here",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {jwtSecurityScheme, Array.Empty<String>() } });

});

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseInMemoryDatabase("LibraryDb"));

// JWT Authentication
var jwtKey = "19b4cbbfe1c17de8df5bb4c6c4078400";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    SeedData(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();


void SeedData(LibraryContext context)
{
    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User { Id = 1, Username = "librarian", Password = "admin123", Role = "Librarian" },
            new User { Id = 2, Username = "client1", Password = "pass123", Role = "Client" },
            new User { Id = 3, Username = "client2", Password = "pass123", Role = "Client" },
            new User { Id = 4, Username = "client3", Password = "pass123", Role = "Client" },
            new User { Id = 5, Username = "client4", Password = "pass123", Role = "Client" },
            new User { Id = 6, Username = "client5", Password = "pass123", Role = "Client" }
        );
        context.Books.AddRange(
            new Book
            {
                Id = 1,
                ISBN = "98754215632",
                Author = "Gayle Laakmann McDowell",
                Title = "Cracking the Coding Interview",
                AvailableCopies = 8,
                TotalCopies = 10
            },
            new Book
            {
                Id = 2,
                ISBN = "98754215632",
                Author = "Gayle Laakmann McDowell",
                Title = "Cracking the Tech Career",
                AvailableCopies = 10,
                TotalCopies = 10                
            },
            new Book
            {
                Id = 3,
                ISBN = "98754215632",
                Author = "Gayle Laakmann McDowell",
                Title = "Cracking the PM Career",
                AvailableCopies = 7,
                TotalCopies = 10
            },
            new Book
            {
                Id = 4,
                ISBN = "98754215632",
                Author = "Gayle Laakmann McDowell",
                Title = "Cracking the PM Interview",
                AvailableCopies = 8,
                TotalCopies = 10                
            });
        context.BorrowRecords.AddRange(
                new BorrowRecord() { Id = 5, BorrowDate = DateTime.Now.AddDays(-2), IsReturned = false, UserId = 5 },
                new BorrowRecord() { Id = 6, BorrowDate = DateTime.Now.AddDays(-10), IsReturned = false, UserId = 6 },
                new BorrowRecord() { Id = 3, BorrowDate = DateTime.Now.AddDays(-2), IsReturned = false, UserId = 2 },
                new BorrowRecord() { Id = 4, BorrowDate = DateTime.Now.AddDays(-10), IsReturned = false, UserId = 3 },
                new BorrowRecord() { Id = 1, BorrowDate = DateTime.Now.AddDays(-2), IsReturned = false, UserId = 2 },
                new BorrowRecord() { Id = 2, BorrowDate = DateTime.Now.AddDays(-10), IsReturned = false, UserId = 3 }
                );
        context.SaveChanges();
    }
}