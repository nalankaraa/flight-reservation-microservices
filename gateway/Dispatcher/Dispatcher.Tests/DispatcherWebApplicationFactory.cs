using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace Dispatcher.Tests;

public class DispatcherWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRouteRepository>();
            services.RemoveAll<IRouteResolver>();
            services.RemoveAll<IRequestForwarder>();

            services.AddSingleton<IRouteRepository, InMemoryRouteRepository>();
            services.AddSingleton<IRouteResolver, DatabaseRouteResolver>();
            services.AddSingleton<IRequestForwarder, FakeRequestForwarder>();
        });
    }
}
