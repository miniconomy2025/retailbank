using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using RetailBank.Endpoints;
using RetailBank.Services;
using RetailBank;
using RetailBank.Repositories;
using RetailBank.ExceptionHandlers;
using RetailBank.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Json options for SwaggerUI
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services
    .AddMemoryCache()
    .AddSingleton<ILedgerRepository, TigerBeetleRepository>()
    .AddHostedService<SimulationRunner>()
    .AddServices()
    .AddValidators()
    .AddExceptionHandlers();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.AddEndpoints();

app.UseStatusCodePages();

app.Run();
