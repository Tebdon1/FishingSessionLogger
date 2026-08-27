using Microsoft.EntityFrameworkCore;

using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Catches;
using SessionLogger.Domain.Files;
using SessionLogger.Domain.Methods;
using SessionLogger.Domain.Rigs;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.Tickets;
using SessionLogger.Domain.Venues;
using SessionLogger.Domain.SpeciesTypes;

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
    public DbSet<Catch> Catches { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Bait> Baits { get; set; }
    public DbSet<Species> Species { get; set; }
    public DbSet<Method> Methods { get; set; }
    public DbSet<Rig> Rigs { get; set; }
    public DbSet<File> Files { get; set; }

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
            b.Property(x => x.StartDateTime).IsRequired();
            b.Property(x => x.EndDateTime).IsRequired();
            b.Property(x => x.VenueId).IsRequired();
            b.Property(x => x.Notes).HasMaxLength(2000);

            // A venue with existing sessions logged against it can't be deleted
            b.HasOne(x => x.Venue)
                .WithMany()
                .HasForeignKey(x => x.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Catch>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Catch", SessionLoggerConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.SessionId).IsRequired();
            b.Property(x => x.SpeciesId).IsRequired();

            // Deleting a session deletes its catches
            b.HasOne(x => x.Session)
                .WithMany(x => x.Catches)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // A species/bait with existing catches logged against it can't be deleted
            b.HasOne(x => x.Species)
                .WithMany()
                .HasForeignKey(x => x.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Bait)
                .WithMany()
                .HasForeignKey(x => x.BaitId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Property(x => x.LengthMm).HasPrecision(8, 2);
            b.Property(x => x.WeightG).HasPrecision(8, 2);
            b.Property(x => x.Notes).HasMaxLength(2000);

            // A method with existing catches logged against it can't be deleted
            b.HasOne(x => x.Method)
                .WithMany()
                .HasForeignKey(x => x.MethodId)
                .OnDelete(DeleteBehavior.Restrict);

            // A rig with existing catches logged against it can't be deleted
            b.HasOne(x => x.Rig)
                .WithMany()
                .HasForeignKey(x => x.RigId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<File>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "File", SessionLoggerConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.CatchId).IsRequired();
            b.Property(x => x.FileName).HasMaxLength(255);
            b.Property(x => x.Extension).HasMaxLength(10);

            // One photo per catch; deleting a catch deletes its photo
            b.HasOne(x => x.Catch)
                .WithOne(x => x.Photo)
                .HasForeignKey<File>(x => x.CatchId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.CatchId).IsUnique();
        });

        builder.Entity<Bait>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Bait", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.BaitType).IsRequired();
            b.Property(x => x.Brand).HasMaxLength(255);
            b.Property(x => x.Range).HasMaxLength(255);
            b.Property(x => x.Colour).HasMaxLength(100);
            b.Property(x => x.Flavour).HasMaxLength(100);
            b.Property(x => x.SizeMm).HasPrecision(8, 2);
            b.ConfigureByConvention(); //auto configure for the base class props

            b.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Venue>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Venue", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired();
            b.ConfigureByConvention(); //auto configure for the base class props

            // A ticket with existing venues referencing it can't be deleted
            b.HasOne(x => x.Ticket)
                .WithMany()
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Ticket>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Ticket", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired();
            b.ConfigureByConvention(); //auto configure for the base class props
        });

        builder.Entity<Species>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Species", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.IsSaltwater).IsRequired().HasDefaultValue(false);
            b.ConfigureByConvention(); //auto configure for the base class props

            b.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Method>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Method", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.ConfigureByConvention(); //auto configure for the base class props

            b.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Rig>(b =>
        {
            b.ToTable(SessionLoggerConsts.DbTablePrefix + "Rig", SessionLoggerConsts.DbSchema);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.LengthMm).HasPrecision(8, 2);
            b.Property(x => x.HookSize).HasMaxLength(50);
            b.Property(x => x.HookWeightG).HasPrecision(8, 2);
            b.Property(x => x.HookPattern).HasMaxLength(100);
            b.Property(x => x.Materials).HasMaxLength(255);
            b.Property(x => x.Notes).HasMaxLength(2000);
            b.ConfigureByConvention(); //auto configure for the base class props

            // Not unique, unlike the other lookups - the same rig pattern name (e.g.
            // "Ronnie Rig") is legitimately used for multiple variations that differ
            // by length/hook/materials, so name alone can't be a unique key.
            b.HasIndex(x => x.Name);
        });
    }
}
