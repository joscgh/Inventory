using Inventory.API.Data;
using Inventory.API.Repositories;
using Inventory.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

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

        // Usa migraciones EF Core para mantener el esquema de la BD actualizado.
        context.Database.EnsureCreated();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de la base de datos.");
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
