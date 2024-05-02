using DevIO.Api.Extensions;
using Elmah.Io.Extensions.Logging;

namespace DevIO.Api.Configuration
{
    public static class LoggerConfig
    {
        public static IServiceCollection AddLoggingConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "5b7d49a0a6d142daa5b9eb1b8f2405cc";
                o.LogId = new Guid("2d042e1b-2c26-4633-8dff-261478ef1a3f");
            });

            services.AddHealthChecks()
                .AddElmahIoPublisher(options =>
                {
                    options.ApiKey = "5b7d49a0a6d142daa5b9eb1b8f2405cc";
                    options.LogId = new Guid("2d042e1b-2c26-4633-8dff-261478ef1a3f");
                    options.HeartbeatId = "e8627e9256bb4215b2dda985572bd35e";

                })
                .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

            services.AddHealthChecksUI()
                .AddSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));

            return services;
        }

        public static IApplicationBuilder UseLoggingConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            return app;
        }
    }
}
