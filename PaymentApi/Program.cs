using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentApi.Data;
using PaymentApi.Repositories;
using PaymentApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});
builder.Services.AddControllers();


builder.Services.AddDbContext<PaymentsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=payments.db"),
    ServiceLifetime.Scoped);

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IDemoPaymentGateway, DemoPaymentGateway>();
builder.Services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

builder.Services.AddScoped<IReferenceGenerator, DailySequentialReferenceGenerator>();

builder.Services.AddScoped<IAuthService, AuthService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceWithVeryLongSecretKeyForDevOnly!ChangeInProd";
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };

    // 👇 This tells JWT bearer authentication to read the access token FROM COOKIE
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Try to read "accessToken" cookie
            var token = context.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }
    };
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment API - Enterprise Auth", Version = "v1" });
    var serverUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:7000";
    c.AddServer(new OpenApiServer { Url = serverUrl });
    var security = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    };
    c.AddSecurityDefinition("Bearer", security);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { security, new string[] { } } });
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.EnsureCreated();

    // Seed admin user if not exists
    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var admin = userRepo.GetByUsernameAsync("admin").GetAwaiter().GetResult();
    if (admin == null)
    {
        userRepo.CreateAsync(new PaymentApi.Models.User
        {
            Username = "admin",
            PasswordHash = PaymentApi.Services.AuthService.HashPassword("Admin@123")
        }).GetAwaiter().GetResult();
    }
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
