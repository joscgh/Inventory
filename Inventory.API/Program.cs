using Inventory.API.Data;
using Inventory.API.Repositories;
using Inventory.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var apiUrl = builder.Configuration["Api:Url"] ?? builder.Configuration["Urls"] ?? "http://0.0.0.0:5130";
builder.WebHost.UseUrls(apiUrl);

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

// Coloca esto justo antes de UseAuthorization y MapControllers
app.UseCors("BlazorWasmPolicy");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Evita que el arranque falle cuando la base de datos todavía no está alineada
        // con el modelo actual o cuando las migraciones no se han aplicado por completo.
        // Si la migración no puede aplicarse, la API seguirá arrancando y el error queda
        // registrado para corregirse de forma explícita.
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        if (pendingMigrations.Count > 0)
        {
            logger.LogWarning("Migraciones pendientes de EF Core: {PendingMigrations}", string.Join(", ", pendingMigrations));
            logger.LogWarning("Se aplicará EnsureCreated para dejar la API operativa con el modelo actual.");
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            await context.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de la base de datos. La API continuará arrancando con el estado actual de la conexión.");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
