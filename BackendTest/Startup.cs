using Backend;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTest
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<InMemoryDbFactory<Program>>();
        }
    }
}
