using ExamesAPI.Infra;
using ExamesAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RepositorioMemoria>();
builder.Services.AddScoped<ResultadoService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

// Seed simples para a demo (dados fictícios — nenhum dado real de paciente).
SeedData.Popular(app.Services.GetRequiredService<RepositorioMemoria>());

app.Run();

// Exposto para eventuais testes de integração.
public partial class Program { }
