using System.Text;
using Auth.Api.Data;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Roboteasy Auth API",
        Version = "v1",
        Description = "Registro, login JWT e listagem de usuarios."
    });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT. Ex: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    opt.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddSingleton<TokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Configure Jwt:Key");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
         .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (var i = 0; i < 10; i++)
    {
        try
        {
            db.Database.EnsureCreated();
            break;
        }
        catch
        {
            Thread.Sleep(2000);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "auth" }));

app.Run();
