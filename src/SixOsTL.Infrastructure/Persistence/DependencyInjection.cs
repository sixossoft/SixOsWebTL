using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixOsTL.Application.Common.Interfaces;
using SixOsTL.Infrastructure.Services;
using SixOsTL.Infrastructure.Settings;

namespace SixOsTL.Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SixOsTLDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("Connection"),
                    b => b.MigrationsAssembly(typeof(SixOsTLDbContext).Assembly.FullName)
                ));

            services.AddScoped<IApplicationDbContext>(p => p.GetRequiredService<SixOsTLDbContext>());

            // App services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITaiLieuService, TaiLieuService>();

            // FTP service
            services.Configure<FtpSettings>(configuration.GetSection(FtpSettings.SectionName));
            services.AddScoped<IFtpService, FtpService>();

            return services;
        }
    }
}
