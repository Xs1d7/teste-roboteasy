using System.Security.Claims;
using System.Text;
using Chat.Api;
using Chat.Api.Hubs;
using Chat.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Roboteasy Chat API",
        Version = "v1",
        Description =
            "REST: usuarios online e historico. Tempo real via SignalR em /hubs/chat " +
            "(nao aparece no Swagger — ver docs/04-chat.md)."
    });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT. Ex: Bearer {token} ou ?access_token= no hub.",
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
builder.Services.AddSingleton<MessageStore>();
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var log = sp.GetRequiredService<ILogger<EventBus>>();
    return EventBus.CreateAsync(cfg, log).GetAwaiter().GetResult();
});
builder.Services.AddHostedService<EventConsumer>();

var redisCs = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisCs))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisCs));
    builder.Services.AddSingleton<IPresenceTracker, RedisPresenceTracker>();
    builder.Services
        .AddSignalR()
        .AddStackExchangeRedis(redisCs, opt =>
        {
            opt.Configuration.ChannelPrefix = RedisChannel.Literal("roboteasy-signalr");
        });
}
else
{
    builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();
    builder.Services.AddSignalR();
}

builder.Services.AddSingleton<IUserIdProvider, NameIdProvider>();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = ClaimTypes.Name
        };

        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/chat"))
                {
                    ctx.Token = accessToken;
                }

                return Task.CompletedTask;
            }
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "chat" }));

app.Run();
