using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using RetailBank.Endpoints;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using RetailBank.Services;
using TigerBeetle;
using RetailBank;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOptions<SimulationOptions>()
    .BindConfiguration(SimulationOptions.Section);

builder.Services.AddSingleton<ITigerBeetleClientProvider, TigerBeetleClientProvider>();
builder.Services.AddSingleton<ITransactionService, TransactionService>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<ILoanService, LoanService>();
builder.Services.AddSingleton<ILedgerRepository, TigerBeetleRepository>();
builder.Services.AddSingleton<ISimulationControllerService, SimulationControllerService>();
builder.Services.AddHostedService<SimulationRunner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.AddAccountEndpoints();
app.AddSimulationEndpoints();

app.Run();

