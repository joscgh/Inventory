using Inventory.API.Data;
using Inventory.API.Repositories;
using Inventory.API.Services;
using Inventory.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Los endpoints se declaran en appsettings.json bajo "Kestrel:Endpoints", que es
// donde Kestrel los lee por sí solo. Son dos, a propósito:
//   Http  0.0.0.0:5130 -> app nativa (MAUI) y herramientas locales.
//   Https 0.0.0.0:5114 -> la PWA, que la sirve esta misma API (mismo origen).
// 0.0.0.0 significa "todas las interfaces", así que acepta conexiones del teléfono.
//
// Por código sólo se resuelve lo que la configuración no puede:
//   * si falta el certificado, se cae a HTTP para que la API arranque igual;
//   * un override explícito con Api:Url o --urls.
var certPath = builder.Configuration["Kestrel:Certificates:Default:Path"];
var certResolvedPath = string.IsNullOrWhiteSpace(certPath)
    ? null
    : Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, certPath));
var httpsConfigured = builder.Configuration["Kestrel:Endpoints:Https:Url"] is not null;
var httpsAvailable = httpsConfigured && certResolvedPath is not null && File.Exists(certResolvedPath);

var explicitUrls = builder.Configuration["Api:Url"] ?? builder.Configuration["Urls"];
if (!string.IsNullOrWhiteSpace(explicitUrls))
{
    builder.WebHost.UseUrls(explicitUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
else if (httpsConfigured && !httpsAvailable)
{
    // UseUrls anula "Kestrel:Endpoints", así que deja sólo el endpoint HTTP.
    var httpOnly = builder.Configuration["Kestrel:Endpoints:Http:Url"] ?? "http://0.0.0.0:5130";
    builder.WebHost.UseUrls(httpOnly);
}

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IAdjustmentRepository, AdjustmentRepository>();
builder.Services.AddScoped<IItemStockRepository, ItemStockRepository>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

builder.Services.AddScoped<ITaxRepository, TaxRepository>();
builder.Services.AddScoped<ITaxService, TaxService>();

builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<INoteService, NoteService>();

builder.Services.AddScoped<ICustomerAccountRepository, CustomerAccountRepository>();
builder.Services.AddScoped<ICustomerAccountService, CustomerAccountService>();
builder.Services.AddScoped<ICustomerAccountUserRepository, CustomerAccountUserRepository>();
builder.Services.AddScoped<ICustomerAccountUserService, CustomerAccountUserService>();
builder.Services.AddScoped<IAccountLocationRepository, AccountLocationRepository>();
builder.Services.AddScoped<IAccountLocationService, AccountLocationService>();
builder.Services.AddScoped<IAccountLogoRepository, AccountLogoRepository>();
builder.Services.AddScoped<IAccountLogoService, AccountLogoService>();

// Facturación fiscal: cajas, rangos de numeración y facturas.
builder.Services.AddScoped<ITerminalRepository, TerminalRepository>();
builder.Services.AddScoped<ITerminalService, TerminalService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentProvider, ManualPaymentProvider>();
builder.Services.Configure<UbiiOptions>(builder.Configuration.GetSection("Ubii"));
builder.Services.AddHttpClient<UbiiApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<UbiiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IExchangeRateScraper, BcvExchangeRateScraper>(client =>
{
    client.BaseAddress = new Uri("https://www.bcv.org.ve/");
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorWasmPolicy", policy =>
    {
        policy.AllowAnyOrigin() // El puerto donde corra tu Blazor WASM
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (httpsConfigured && !httpsAvailable)
{
    app.Logger.LogWarning(
        "No se encontró el certificado {CertPath}: la API sólo escuchará en HTTP y la PWA no podrá servirse por HTTPS. Genera el certificado con certs\\create-local-dev-cert.ps1.",
        certResolvedPath);
}

// Imprime las direcciones de la LAN para no tener que adivinar qué escribir en el
// teléfono ni en la pantalla Servidor del APK.
app.Lifetime.ApplicationStarted.Register(() =>
{
    // Sólo Ethernet y Wi-Fi: así no aparecen las interfaces de VPN o túneles, que
    // están activas pero no son la dirección por la que llega el teléfono.
    var lanAddresses = NetworkInterface.GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up
                      && (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                          || nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                          || nic.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet))
        .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
        .Select(addr => addr.Address)
        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork
                     && !IPAddress.IsLoopback(ip)
                     && !ip.ToString().StartsWith("169.254.", StringComparison.Ordinal))
        .Select(ip => ip.ToString())
        .Distinct()
        .ToList();

    if (lanAddresses.Count == 0)
    {
        app.Logger.LogWarning("No se detectó ninguna dirección IPv4 de red local.");
        return;
    }

    var httpUrl = builder.Configuration["Kestrel:Endpoints:Http:Url"];
    var httpsUrl = builder.Configuration["Kestrel:Endpoints:Https:Url"];

    foreach (var ip in lanAddresses)
    {
        if (httpsAvailable && !string.IsNullOrWhiteSpace(httpsUrl))
        {
            app.Logger.LogInformation("PWA (navegador del teléfono): {Url}",
                httpsUrl.Replace("0.0.0.0", ip, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(httpUrl))
        {
            app.Logger.LogInformation("APK (pantalla Servidor): {Url}",
                httpUrl.Replace("0.0.0.0", ip, StringComparison.Ordinal));
        }
    }
});

// Coloca esto justo antes de UseAuthorization y MapControllers
app.UseCors("BlazorWasmPolicy");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Aplica las migraciones pendientes también cuando la base ya existe. EnsureCreated
        // no actualiza un esquema existente y dejaría fuera columnas nuevas como EmissionMode.
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        if (pendingMigrations.Count > 0)
        {
            logger.LogWarning("Migraciones pendientes de EF Core: {PendingMigrations}", string.Join(", ", pendingMigrations));
        }

        await context.Database.MigrateAsync();

        // Compatibilidad con bases creadas antes de la migración de métodos de pago.
        // Es idempotente y evita que la pantalla de cajas falle mientras se actualiza
        // una instalación existente.
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "TerminalPaymentMethods" (
                "Id" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                "TerminalId" integer NOT NULL,
                "Code" text NOT NULL,
                "Name" text NOT NULL,
                "IsActive" boolean NOT NULL,
                CONSTRAINT "FK_TerminalPaymentMethods_Terminals_TerminalId"
                    FOREIGN KEY ("TerminalId") REFERENCES "Terminals" ("Id") ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TerminalPaymentMethods_TerminalId_Code"
                ON "TerminalPaymentMethods" ("TerminalId", "Code");
            """);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de la base de datos. La API continuará arrancando con el estado actual de la conexión.");
    }
}


// La API también sirve la PWA (Inventory.APP) desde el mismo origen. Es lo que
// evita el problema de fondo al usarla desde el teléfono: si la app y la API
// están en orígenes distintos, el navegador exige confiar en el certificado de
// cada uno, y cuando el del origen de la API no es de confianza aborta las
// peticiones en silencio (parece un timeout, sin ningún aviso).
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// Cualquier ruta que no sea de la API la resuelve el enrutador de Blazor.
app.MapFallbackToFile("index.html");

app.Run();
