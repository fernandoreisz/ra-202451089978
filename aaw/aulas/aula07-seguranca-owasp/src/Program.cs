using System.Text;
using ApiVazada.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FALHA 3 (não corrija ainda!)
// Segredos hardcoded + DeveloperExceptionPage sempre ativo.

var conn = builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(conn))
{
    throw new InvalidOperationException(
        "Connection string não configurada.");
}

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key não configurada.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(conn));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(jwtKey)
        ),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// FALHA 3 (não corrija ainda!)

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/erro");
}

app.Map("/erro", () => Results.Problem("Erro interno."));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();