using System.Text;
using Serilog;
using Serilog.Events;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Seed;

using LoanDeductionPrediction.API.Middleware;
using LoanDeductionPrediction.Repositories.UnitOfWork;

using LoanDeductionPrediction.Repositories.Implementations;
using LoanDeductionPrediction.Repositories.Interfaces;

using LoanDeductionPrediction.Services.BackgroundServices;
using LoanDeductionPrediction.Services.Implementations;
using LoanDeductionPrediction.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

// LOGGING(Serilog is a logging library for .NET applications that provides structured logging capabilities. 
// It allows developers to log messages with various levels of severity (e.g., Information, Warning, Error).


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
  .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

 
// CONTROLLERS(it will receive and process HTTP requests and return HTTP responses) 

builder.Services.AddControllers();

 
// DATABASE
 

builder.Services.AddDbContext<LoanDeductionDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

 
// AUTOMAPPER(it automatically maps data from one object to another, between DTOs and entities)
 

builder.Services.AddAutoMapper(
    cfg => { },
    AppDomain.CurrentDomain.GetAssemblies());

 
// REPOSITORIES REGISTRATION(AddScope create a new instance for each HTTP request)
 

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
    IPaymentBehaviorRepository,
    PaymentBehaviorRepository>();

builder.Services.AddScoped<
    IRiskPredictionRepository,
    RiskPredictionRepository>();

builder.Services.AddScoped<
    IAlertRepository,
    AlertRepository>();

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
    ILoanRequestRepository,
    LoanRequestRepository>();

builder.Services.AddScoped<
    IBorrowerLoanApplicationRepository,
    BorrowerLoanApplicationRepository>();

    // CLOCK SERVICE

var useTestClock =
    builder.Configuration.GetValue<bool>(
        "Clock:UseTestClock");

if (useTestClock)
{
    builder.Services.AddSingleton<IClock>(
        new TestClock
        {
            Today =
                builder.Configuration.GetValue<DateOnly>(
                    "Clock:TestDate")
        });
}
else
{
    builder.Services.AddSingleton<
//Singleton handles whole application with one instance of the service.
        IClock,
        SystemClock>();
}

// SERVICES REGISTRATION(AddScope create a new instance for each HTTP request.)
 

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
    IAlertService,
    AlertService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.AddScoped<
    IRefreshTokenService,
    RefreshTokenService>();

builder.Services.AddScoped<
    ILoanRequestService,
    LoanRequestService>();

builder.Services.AddScoped<
    IBorrowerLoanApplicationService,
    BorrowerLoanApplicationService>();
 
// BACKGROUND SERVICES (Automatically checks overdue repayment schedules and records MISSED payment behavior.)

builder.Services.AddHostedService<
    PaymentBehaviorBackgroundService>();

 
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

 
// AUTHORIZATION (It defines the access and rules for users to perform defined actions within the application.)
 

builder.Services.AddAuthorization();

 
// CORS(Cross Origin Resource Sharing. It aloows frotend to access backend API from different port.)
 

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
using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<LoanDeductionDbContext>();

    context.Database.Migrate();

    DatabaseSeeder.Seed(context);
}
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

 
// HTTPS
 

app.UseHttpsRedirection();

 
// CORS
 

app.UseCors(
    "AllowFrontend");

 
// AUTHENTICATION
 

app.UseAuthentication();

 
// AUTHORIZATION
 

app.UseAuthorization();

 
// CONTROLLERS
 

app.MapControllers();

 
// RUN APPLICATION
 

app.Run();