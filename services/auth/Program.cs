using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Auth.Api.Configuration;
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

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageOptions>>().Value;
    var config = new AmazonS3Config
    {
        ServiceURL = opts.ServiceUrl,
        ForcePathStyle = opts.ForcePathStyle,
        AuthenticationRegion = "us-east-1"
    };
    var credentials = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);
    return new AmazonS3Client(credentials, config);
});
builder.Services.AddScoped<IStorageService, S3StorageService>();

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
            // Banco ja existente (EnsureCreated nao altera schema): garante coluna AvatarKey
            await db.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "AvatarKey" character varying(255) NULL;""");
            break;
        }
        catch
        {
            Thread.Sleep(2000);
        }
    }

    var storage = scope.ServiceProvider.GetRequiredService<IStorageService>();
    var storageOpts = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageOptions>>().Value;
    for (var i = 0; i < 10; i++)
    {
        try
        {
            await storage.EnsureBucketAsync(storageOpts.AvatarBucket);
            break;
        }
        catch
        {
            Thread.Sleep(2000);
        }
    }
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue("Swagger:Enabled", false))
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
