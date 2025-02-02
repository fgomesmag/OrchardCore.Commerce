using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Payment.Abstractions;
using OrchardCore.Commerce.Payment.Constants;
using OrchardCore.Commerce.Services;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Commerce.Payment;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<ResourceManagementOptions>, ResourceManagementOptionsConfiguration>();
        services.AddScoped<IPaymentService, PaymentService>();
    }
}

[Feature(FeatureIds.DummyProvider)]
public class DummyProviderStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services) =>
        services.AddScoped<IPaymentProvider, DummyPaymentProvider>();
}
