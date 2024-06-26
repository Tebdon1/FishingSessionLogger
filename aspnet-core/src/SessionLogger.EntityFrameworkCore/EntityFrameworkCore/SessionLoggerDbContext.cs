﻿using Microsoft.EntityFrameworkCore;

using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Folders;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.Tickets;
using SessionLogger.Domain.Venues;

using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace SessionLogger.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class SessionLoggerDbContext :
    AbpDbContext<SessionLoggerDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<Session> Sessions { get; set; }
    public DbSet<CatchSummary> CatchSummaries { get; set; }
    public DbSet<CatchDetail> CatchDetails { get; set; }
    public DbSet<CatchWeight> CatchWeights { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Bait> Baits { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }

    public DbSet<UserView> UserViews { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public SessionLoggerDbContext(DbContextOptions<SessionLoggerDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(SessionLoggerConsts.DbTablePrefix + "YourEntities", SessionLoggerConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
        builder.Entity<Session>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Session", SessionLoggerConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.SessionDate).IsRequired();
            b.Property(x => x.Venue).IsRequired().HasMaxLength(128);
        });

        builder.Entity<CatchSummary>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "CatchSummary", SessionLoggerConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
        });

        builder.Entity<CatchDetail>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "CatchDetail", SessionLoggerConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
        });

        builder.Entity<CatchWeight>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "CatchWeight", SessionLoggerConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
        });

        builder.Entity<Bait>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Bait", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired();
            b.ConfigureByConvention(); //auto configure for the base class props
        });

        builder.Entity<Venue>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Venue", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired();
            b.ConfigureByConvention(); //auto configure for the base class props
        });

        builder.Entity<Ticket>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Ticket", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired();
            b.ConfigureByConvention(); //auto configure for the base class props
        });
    }
}
