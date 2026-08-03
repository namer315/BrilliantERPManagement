using FluentNHibernate.Mapping;

namespace CommonData.VO;

/// <summary>
/// Base entity with audit fields. All persistent entities inherit from this.
/// Uses union-subclass mapping strategy — each concrete entity gets its own table
/// with all base columns included.
/// </summary>
public abstract class AppBaseEntity
{
    public virtual long Id { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual string CreatedBy { get; set; } = "system";
    public virtual string UpdatedBy { get; set; } = "system";
    public virtual bool IsDeleted { get; set; }
}

/// <summary>
/// NHibernate mapping for AppBaseEntity via union-subclass strategy.
/// Subclass mappings use SubclassMap&lt;T&gt; in their own VO files.
/// </summary>
public class AppBaseEntityMap : ClassMap<AppBaseEntity>
{
    public AppBaseEntityMap()
    {
        UseUnionSubclassForInheritanceMapping();

        Id(x => x.Id)
            .GeneratedBy.HiLo("1000");

        Map(x => x.CreatedAt)
            .Not.Nullable()
            .Default("CURRENT_TIMESTAMP");

        Map(x => x.UpdatedAt)
            .Nullable();

        Map(x => x.CreatedBy)
            .Not.Nullable()
            .Length(100)
            .Default("'system'");

        Map(x => x.UpdatedBy)
            .Not.Nullable()
            .Length(100)
            .Default("'system'");

        Map(x => x.IsDeleted)
            .Not.Nullable()
            .Default("0");
    }
}
