using Inventory.SharedUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.SharedUI
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra los servicios que hablan con Inventory.API. Los consumen todas las
        /// apps (PWA de inventario, PWA de facturación y las nativas/POS), así que la
        /// lista vive aquí y no duplicada en cada Program.cs.
        ///
        /// El HttpClient NO se registra aquí a propósito: cada app resuelve la URL base
        /// de la API a su manera (origen actual en la PWA, preferencia guardada en la
        /// app nativa) y debe registrarlo antes de llamar a este método.
        /// </summary>
        public static IServiceCollection AddInventoryApiServices(this IServiceCollection services)
        {
            services.AddScoped<ItemApiService>();
            services.AddScoped<CurrencyApiService>();
            services.AddScoped<CategoryApiService>();
            services.AddScoped<TaxApiService>();
            services.AddScoped<AdjustmentApiService>();
            services.AddScoped<CustomerAccountApiService>();
            services.AddScoped<ConsumerCustomerApiService>();
            services.AddScoped<AuthApiService>();
            services.AddScoped<UserStateService>();
            services.AddScoped<NoteApiService>();
            services.AddScoped<InvoiceApiService>();
            return services;
        }
    }
}
