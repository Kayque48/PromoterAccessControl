// ========================================
// CONFIGURAÇÃO DA API - PROMOTER ACCESS CONTROL
// ========================================
// Este arquivo configura a API ASP.NET Core, incluindo serviços,
// middlewares e roteamento de endpoints.

var builder = WebApplication.CreateBuilder(args);

// Registra os serviços necessários para a API funcionar
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  // Habilita documentação automática via Swagger

var app = builder.Build();

// Configura o pipeline de processamento de requisições HTTP
app.UseSwagger();      // Disponibiliza a definição OpenAPI
app.UseSwaggerUI();    // Habilita a interface interativa do Swagger

app.UseHttpsRedirection();  // Redireciona requisições HTTP para HTTPS

app.UseAuthorization();

app.MapControllers();  // Mapeia automaticamente os controllers e seus endpoints

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
