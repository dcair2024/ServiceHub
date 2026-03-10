using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ServiceHub.Infrastructure.Data;
using ServiceHub.Infrastructure.Identity;
using ServiceHub.Core.Interfaces;
using ServiceHub.Infrastructure.Services;
using ServiceHub.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURAÇÃO DE SERVIÇOS (DI) ---

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// IMPORTANTE: Alterado para suportar Views (MVC) + Controllers (API)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Necessário para as telas de Login do Identity

// Configuração Swagger (Mantida e Ajustada)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceHub API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Digite: Bearer {seu token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Banco de Dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Injeção de Dependência das suas Interfaces
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// AUTENTICAÇÃO HÍBRIDA (Cookie para o Site + JWT para API)
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
        };
    });

var app = builder.Build();

// --- PIPELINE DE EXECUÇÃO (MIDDLEWARES) ---

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceHub API V1");
        // REMOVIDO: c.RoutePrefix = string.Empty; 
        // Agora o Swagger fica em /swagger e a Home do site fica na raiz /
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // CRÍTICO: Sem isso o CSS do Claudio não carrega!

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 1. MAPEAMENTO DE ROTAS (A ordem aqui é vital)
// Primeiro: Mapear as Razor Pages (Identity)
app.MapRazorPages();

// Segundo: Mapear a rota padrão do MVC (O site da Gemima)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Terceiro: Mapear os Controllers de API (Seu Swagger/JWT)
app.MapControllers();

// SEED DE ROLES (Mantido)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        // Usando Task.Run().Wait() caso o contexto não seja async, 
        // mas o ideal é manter o await se o seu Program.cs permitir
        RoleSeeder.SeedRolesAsync(roleManager).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro no Seed de Roles.");
    }
}

app.Run();