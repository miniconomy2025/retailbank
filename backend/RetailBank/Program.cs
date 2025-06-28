using RetailBank.Endpoints;
using RetailBank.Repositories;
using RetailBank.Services;
using TigerBeetle;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Client>(serviceProvider =>
{
    var tbAddress = Environment.GetEnvironmentVariable("TB_ADDRESS") ?? "4000";
    var clusterID = UInt128.Zero;
    var addresses = new[] { tbAddress };
    var client = new Client(clusterID, addresses);
    return client;
});

builder.Services.AddSingleton<ITransactionService, TransactionService>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddSingleton<ILoanService, LoanService>();
builder.Services.AddSingleton<ILedgerRepository, TigerBeetleRepository>();
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

app.Run();

