using System.Text;
using Serilog;
using Serilog.Events;

using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Implementations;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Repositories.UnitOfWork;

using LoanDeductionPrediction.API.Middleware;

using LoanDeductionPrediction.Services.Implementations;
using LoanDeductionPrediction.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;


// LOGGING


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override(
        "Microsoft",
        LogEventLevel.Information)
    .MinimumLevel.Override(
        "Microsoft.AspNetCore",
        LogEventLevel.Information)
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


// CONTROLLERS


builder.Services.AddControllers();


// DATABASE


builder.Services.AddDbContext<LoanDeductionDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"),
        sqlOptions =>
            sqlOptions.EnableRetryOnFailure()
    ));


// AUTOMAPPER


builder.Services.AddAutoMapper(
    cfg => { },
    AppDomain.CurrentDomain.GetAssemblies());


// REPOSITORIES REGISTRATION


builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    ILoanRepository,
    LoanRepository>();

builder.Services.AddScoped<
    IRepaymentScheduleRepository,
    RepaymentScheduleRepository>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();

builder.Services.AddScoped<
    IPaymentBehaviorRepository,
    PaymentBehaviorRepository>();

builder.Services.AddScoped<
    IRiskPredictionRepository,
    RiskPredictionRepository>();

builder.Services.AddScoped<
    IDashboardRepository,
    DashboardRepository>();

builder.Services.AddScoped<
    IRefreshTokenRepository,
    RefreshTokenRepository>();

builder.Services.AddScoped<
    ILoanDeductionUnitOfWork,
    LoanDeductionUnitOfWork>();

builder.Services.AddScoped<
    IBorrowerLoanApplicationRepository,
    BorrowerLoanApplicationRepository>();


// CLOCK SERVICE
//Singleton- It will create a single object for whole application.


builder.Services.AddSingleton<
    IClock,
    SystemClock>();


// SERVICES REGISTRATION
//Scoped- It will create a new object for each request.

builder.Services.AddScoped<
    IUserService,
    UserService>();

builder.Services.AddScoped<
    ILoanService,
    LoanService>();

builder.Services.AddScoped<
    IRepaymentScheduleService,
    RepaymentScheduleService>();

builder.Services.AddScoped<
    IPaymentBehaviorService,
    PaymentBehaviorService>();

builder.Services.AddScoped<
    IRiskPredictionService,
    RiskPredictionService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    IRefreshTokenService,
    RefreshTokenService>();

builder.Services.AddScoped<
    IBorrowerLoanApplicationService,
    BorrowerLoanApplicationService>();

// JWT AUTHENTICATION


var jwtKey =
    builder.Configuration["Jwt:Key"];

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"];

var jwtAudience =
    builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT key is missing from configuration.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT issuer is missing from configuration.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT audience is missing from configuration.");
}

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateIssuer = true,

                ValidIssuer =
                    jwtIssuer,

                ValidateAudience = true,

                ValidAudience =
                    jwtAudience,

                ValidateLifetime = true,

                ClockSkew =
                    TimeSpan.Zero
            };
    });


// AUTHORIZATION


builder.Services.AddAuthorization();


// CORS controls which frontend applications are allowed to call your backend API.


builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// SWAGGER / OPENAPI


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title =
                "Loan Deduction Prediction API",

            Version =
                "v1",

            Description =
                "Loan Deduction Prediction System with Behavioral Analytics"
        });

    // JWT Bearer Security Definition

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name =
                "Authorization",

            Type =
                SecuritySchemeType.Http,

            Scheme =
                "bearer",

            BearerFormat =
                "JWT",

            In =
                ParameterLocation.Header,

            Description =
                "Enter: Bearer {your JWT token}"
        });

    // JWT Security Requirement
    
    options.AddSecurityRequirement(
        document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        document)
                ] =
                    new List<string>()
            });
});


// BUILD APPLICATION


var app = builder.Build();

app.UseSerilogRequestLogging();


// GLOBAL EXCEPTION HANDLING


app.UseMiddleware<
    GlobalExceptionMiddleware>();


// SWAGGER


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Loan Deduction Prediction API v1");

        options.RoutePrefix =
            "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors(
    "AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();