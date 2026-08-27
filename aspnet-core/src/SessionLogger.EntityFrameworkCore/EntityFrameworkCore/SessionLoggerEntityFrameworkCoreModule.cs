using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.Venues;

namespace SessionLogger.EntityFrameworkCore;

[DependsOn(
    typeof(SessionLoggerDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
public class SessionLoggerEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        SessionLoggerEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SessionLoggerDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);

            options.Entity<Session>(options =>
            {
                options.DefaultWithDetailsFunc = query => query
                .Include(o => o.Venue)
                .Include(o => o.Catches).ThenInclude(c => c.Species)
                .Include(o => o.Catches).ThenInclude(c => c.Bait)
                .Include(o => o.Catches).ThenInclude(c => c.Method)
                .Include(o => o.Catches).ThenInclude(c => c.Rig)
                .Include(o => o.Catches).ThenInclude(c => c.Photo);
            });

            options.Entity<Catch>(options =>
            {
                options.DefaultWithDetailsFunc = query => query
                .Include(o => o.Session)
                .Include(o => o.Species)
                .Include(o => o.Bait)
                .Include(o => o.Method)
                .Include(o => o.Rig)
                .Include(o => o.Photo);
            });

            options.Entity<Venue>(options =>
            {
                options.DefaultWithDetailsFunc = query => query
                .Include(o => o.Ticket);
            });
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also SessionLoggerMigrationsDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });
    }
}
