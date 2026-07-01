using ExamesAPI.Infra;
using ExamesAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<RepositorioMemoria>();
builder.Services.AddScoped<ResultadoService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

// Seed simples para a demo (dados fictícios — nenhum dado real de paciente).
SeedData.Popular(app.Services.GetRequiredService<RepositorioMemoria>());

app.Run();

// Exposto para eventuais testes de integração.
public partial class Program { }
