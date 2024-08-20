using LudoLab_ConnectSys_Server.Data;
using LudoLab_ConnectSys_Server.Interceptors;
using LudoLab_ConnectSys_Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging; // Agrega este using

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<TokenService>();
// Registrar GrupoService
builder.Services.AddScoped<GrupoService>();
// Registrar IHttpContextAccessor para AuditService
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCorsPolicy", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Registrar HttpCliente para AuditService
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<AuditSaveChangesInterceptor>();
builder.Services.AddLogging(); // Configura el logging
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(); // Sirve archivos estaticos desde wwwroot por defecto
// Configurar archivos est�ticos para la carpeta uploads
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthorization();

app.MapControllers();

app.UseCors("DevCorsPolicy");

app.Run();