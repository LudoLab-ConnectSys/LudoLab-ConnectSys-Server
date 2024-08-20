namespace LudoLab_ConnectSys_Server.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DirectorioDeArchivos.Shared;
using Microsoft.Extensions.Logging;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, HttpClient httpClient,
        ILogger<AuditSaveChangesInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
        _logger = logger;
        _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        LogChangesAsync(eventData.Context).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await LogChangesAsync(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task LogChangesAsync(DbContext context)
    {
        if (context == null) return;

        var auditEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .Select(e => CreateAuditEntry(e))
            .ToList();

        foreach (var auditEntry in auditEntries)
        {
            var auditEntryJson = JsonConvert.SerializeObject(auditEntry, _jsonSettings);
            _logger.LogInformation($"Sending audit entry: {auditEntryJson}");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7500/audit")
            {
                Content = JsonContent.Create(auditEntry)
            };

            var idUsuario = _httpContextAccessor.HttpContext?.Request.Headers["idUsuario"].ToString();
            var nombreUsuario = _httpContextAccessor.HttpContext?.Request.Headers["nombreUsuario"].ToString();

            if (!string.IsNullOrEmpty(idUsuario))
            {
                request.Headers.Add("idUsuario", idUsuario);
            }

            if (!string.IsNullOrEmpty(nombreUsuario))
            {
                request.Headers.Add("nombreUsuario", nombreUsuario);
            }

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Audit entry sent successfully.");
            }
            else
            {
                _logger.LogError(
                    $"Failed to send audit entry. StatusCode: {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }

    private AuditLog CreateAuditEntry(EntityEntry entry)
    {
        var auditEntry = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Accion = entry.State.ToString(),
            NombreTabla = entry.Entity.GetType().Name,
            ClavesPrimarias = JsonConvert.SerializeObject(GetPrimaryKeyValues(entry), _jsonSettings),
            ValoresAntiguos = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                ? JsonConvert.SerializeObject(GetModifiedProperties(entry.OriginalValues), _jsonSettings)
                : null,
            ValoresNuevos = entry.State == EntityState.Added || entry.State == EntityState.Modified
                ? JsonConvert.SerializeObject(GetModifiedProperties(entry.CurrentValues), _jsonSettings)
                : null
        };

        if (entry.State == EntityState.Added)
        {
            auditEntry.ValoresAntiguos = null;
        }
        else if (entry.State == EntityState.Deleted)
        {
            auditEntry.ValoresNuevos = null;
        }

        return auditEntry;
    }

    private static object GetPrimaryKeyValues(EntityEntry entry)
    {
        var primaryKey = entry.Properties.Where(p => p.Metadata.IsPrimaryKey())
            .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
        return primaryKey;
    }

    private static object GetModifiedProperties(PropertyValues values)
    {
        var modifiedProperties = values.Properties.ToDictionary(p => p.Name, p => values[p.Name]);
        return modifiedProperties;
    }
}