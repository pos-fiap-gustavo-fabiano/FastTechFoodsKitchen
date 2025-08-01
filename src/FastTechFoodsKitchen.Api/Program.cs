using FastTechFoodsAuth.Security.Extensions;
using FastTechFoodsKitchen.Api.Config;
using FastTechFoodsKitchen.Api.Consumers;
using FastTechFoodsKitchen.Application.Interfaces;
using FastTechFoodsKitchen.Infrastructure.Context;
using FastTechFoodsKitchen.Infrastructure.Extensions;
using FastTechFoodsKitchen.Infrastructure.Repositories;
using FastTechFoodsKitchen.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Add MongoDB
builder.Services.AddSingleton<ApplicationDbContext>();
builder.Services.AddSingleton<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

// Add Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// Add Services
builder.Services.AddScoped<IKitchenOrderService, FastTechFoodsKitchen.Application.Services.KitchenOrderService>();
builder.Services.AddScoped<IAnalyticsService, FastTechFoodsKitchen.Application.Services.AnalyticsService>();

// Add Catalog Service with Polly retry pattern

// Add MassTransit for messaging
builder.Services.AddRabbitMQConfiguration();

// Add HttpClient for Orders API
builder.Services.AddHttpClient<IOrdersApiClient, OrdersApiClient>();

StartUpConfig.AddObservability(builder);
// Add RabbitMQ Consumers
builder.Services.AddScoped<IOrderCreatedMessageHandler, OrderCreatedMessageHandler>();
builder.Services.AddHostedService<OrderCreatedMessageConsumer>();
builder.Services.AddScoped<IOrderCancelledMessageHandler, OrderCancelledMessageHandler>();
builder.Services.AddHostedService<OrderCancelledMessageConsumer>();
builder.Services.AddScoped<IOrderUserCancelledMessageHandler, OrderUserCancelledMessageHandler>();
builder.Services.AddHostedService<OrderUserCancelledMessageConsumer>();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddFastTechFoodsSwaggerWithJwt("FastTechFoodsKitchen API", "v1", "API para controle da cozinha");
builder.Services.AddFastTechFoodsJwtAuthentication(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseFastTechFoodsSecurityAudit();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.UseCors("AllowAll");
StartUpConfig.UseObservability(app);

// Mapear health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
