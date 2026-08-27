using SessionLogger.Domain.Baits;
using SessionLogger.Domain.Files;
using SessionLogger.Domain.Methods;
using SessionLogger.Domain.Rigs;
using SessionLogger.Domain.Sessions;
using SessionLogger.Domain.SpeciesTypes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Catches;

public class Catch : AuditedAggregateRoot<int>
{
    public Catch(int id) : base(id)
    {
    }

    public Catch()
    {
    }

    [Required]
    public int SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

    [Required]
    public int SpeciesId { get; set; }

    [ForeignKey(nameof(SpeciesId))]
    public virtual Species Species { get; set; }

    public int? BaitId { get; set; }

    [ForeignKey(nameof(BaitId))]
    public virtual Bait? Bait { get; set; }

    // Not every fish gets weighed, e.g. bulk-logged catches. Always grams, converted
    // from whatever the caller entered (lb+oz) - so every comparison/aggregation is a
    // plain numeric one, with no unit-conversion logic needed at query time (same
    // approach as Bait.SizeMm / Catch.LengthMm).
    public decimal? WeightG { get; set; }

    // The unit WeightG was originally entered in, kept purely for display (e.g. show
    // "2.5kg" instead of silently reformatting to "5lb 8oz") - never used for
    // comparisons. Null for rows created before this field existed.
    public WeightUnit? WeightUnit { get; set; }

    public int? MethodId { get; set; }

    [ForeignKey(nameof(MethodId))]
    public virtual Method? Method { get; set; }

    // Method and Rig are independent - the same rig can be fished under different
    // methods (e.g. Ronnie Rig on a Helicopter or a Lead Clip), so they're separate
    // lookups rather than one implying the other.
    public int? RigId { get; set; }

    [ForeignKey(nameof(RigId))]
    public virtual Rig? Rig { get; set; }

    // Always millimetres, converted from whatever unit the caller entered - so every
    // comparison/aggregation is a plain numeric one, with no unit-conversion logic
    // needed at query time (same approach as Bait.SizeMm).
    public decimal? LengthMm { get; set; }

    // The unit LengthMm was originally entered in, kept purely for display - never
    // used for comparisons. Null for rows created before this field existed.
    public SizeUnit? LengthUnit { get; set; }

    // Time of day the catch was made. Date is implied by the session it belongs to.
    // TimeSpan rather than TimeOnly - this EF Core version (7) has no native TimeOnly
    // mapping support, and TimeSpan already has to be the DTO type anyway since
    // Application.Contracts also targets netstandard2.0, which lacks TimeOnly.
    public TimeSpan? CatchTime { get; set; }

    // One optional photo per catch, owned by the catch (see File.CatchId)
    public virtual File? Photo { get; set; }

    // Free-form commentary specific to this fish, separate from Session.Notes (which
    // covers the session as a whole) - e.g. "foul-hooked", "new PB", "fought for 10 min".
    public string? Notes { get; set; }
}
