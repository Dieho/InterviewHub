// Program.cs

using System.Text;
using System.Text.Json.Serialization;
using InterviewHub.Api.Contracts;
using InterviewHub.Api.Services;
using Microsoft.EntityFrameworkCore;
using InterviewHub.Data;
using InterviewHub.Data.Options;
using InterviewHub.Data.Service;
using InterviewHub.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(8080, listenOptions =>
//     {
//         listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
//     });
// });

builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<QuestionNotifier>();
builder.Services.AddGrpc();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AngularDev");
app.UseAuthentication();
app.UseAuthorization();


app.MapGrpcService<ProgressGrpcService>();

app.MapPost("/auth/register", async (IAuthRepository db, ITokenService tokenService, AuthContracts.RegisterRequest request) =>
{
    var existingUser = await db.GetUserByEmail(request.Email);
    if (existingUser is not null)
        return Results.BadRequest($"User with email {request.Email} already exists");

    var user = new User(request.Email, request.Password);
    await db.AddUser(user);

    var token = tokenService.GenerateToken(user);
    return Results.Created("/auth/register", new AuthContracts.AuthResponse(token, user.Email, user.Role.ToString()));
});

app.MapPost("/auth/login", async (IAuthRepository db, ITokenService tokenService, AuthContracts.LoginRequest request) =>
{
    var passwordHasher = new PasswordHasher<User>(); 
    var existingUser = await db.GetUserByEmail(request.Email);
    if (existingUser is null)
        return Results.Unauthorized();

    var validationResult = passwordHasher.VerifyHashedPassword(existingUser, existingUser.PasswordHash, request.Password);

    if (validationResult == PasswordVerificationResult.Success )
    {
        var token = tokenService.GenerateToken(existingUser);
        return Results.Ok(new AuthContracts.AuthResponse(token, existingUser.Email, existingUser.Role.ToString()));
    }
    else
    {   
        return Results.Unauthorized();
    }
});

app.MapGet("/", () => "Interview Hub API is running");

app.MapGet("/questions", async (IQuestionRepository db) =>
{
    var questions = await db.GetQuestions();
    var response = questions.Select(q => new QuestionResponse(
        q.Id, q.Text, q.Answer, q.Difficulty, q.IsReviewed, q.CategoryId, q.Category.Name));
    return Results.Ok(response);
});

app.MapGet("/questions/{id:guid}", async (IQuestionRepository db, Guid id) =>
{
    var question = await db.GetQuestion(id);
    return question is null ? Results.NotFound() : Results.Ok( new QuestionResponse(question.Id, question.Text, question.Answer, question.Difficulty, question.IsReviewed, question.CategoryId, question.Category.Name));
});

app.MapPost("/questions", async (IQuestionRepository db, QuestionNotifier notifier, CreateQuestionRequest request) =>
{
    var category = await db.GetCategory(request.CategoryId);
    if (category is null)
        return Results.BadRequest($"Category {request.CategoryId} not found");

    var question = new Question(request.Text, request.Answer, request.Difficulty, category);
    await db.AddQuestion(question);

    // публікуємо подію для всіх підписаних gRPC-клієнтів
    await notifier.PublishAsync(new InterviewHub.Api.Grpc.QuestionNotification
    {
        Id = question.Id.ToString(),
        Text = question.Text,
        CategoryName = category.Name
    });

    var response = new QuestionResponse(question.Id, question.Text, question.Answer,
        question.Difficulty, question.IsReviewed, question.CategoryId, category.Name);
    return Results.Created($"/questions/{question.Id}", response);
}).RequireAuthorization();

app.MapGet("/categories", async (IQuestionRepository db) =>
{
    var categories = await db.GetCategories();
    var response = categories.Select(c => new CategoryResponse(c.Id, c.Name));
    return Results.Ok(response);
});

app.MapGet("/categories/{id:guid}", async (IQuestionRepository db, Guid id) =>
{
    var category = await db.GetCategory(id);
    return category is null ? Results.NotFound() : Results.Ok(new CategoryResponse(category.Id, category.Name));
});

app.MapPost("/categories", async (IQuestionRepository db, CreateCategoryRequest request) =>
{
    var category = new Category(request.Name);
    await db.AddCategory(category);
    return Results.Created($"/categories/{category.Id}", new CategoryResponse(category.Id, category.Name));
}).RequireAuthorization(x=> x.RequireRole("Admin"));

app.Run();