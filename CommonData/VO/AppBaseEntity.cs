using FluentNHibernate.Mapping;

namespace CommonData.VO;

/// <summary>
/// Base entity with audit fields. All persistent entities inherit from this.
/// Concrete entities map their own tables via <see cref="EntityWithIdMapping{T}"/>.
/// </summary>
public abstract class AppBaseEntity
{
    public virtual Guid Id { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual string CreatedBy { get; set; } = "system";
    public virtual string UpdatedBy { get; set; } = "system";
    public virtual bool IsDeleted { get; set; }
}

/// <summary>
/// Reusable base mapping for entities deriving from <see cref="AppBaseEntity"/>.
/// Maps the Guid primary key (GuidComb), audit columns and the optimistic-lock
/// Version column. Concrete entities derive from this and map their own columns
/// plus their Table in their own mapping class.
/// </summary>
/// <typeparam name="T">A concrete entity type deriving from AppBaseEntity.</typeparam>
public class EntityWithIdMapping<T> : ClassMap<T> where T : AppBaseEntity
{
    public EntityWithIdMapping()
    {
        base.Cache.NonStrictReadWrite();

        Id(e => e.Id)
            .GeneratedBy.GuidComb();

        OptimisticLock.Version().DynamicUpdate();
    }
}

/// <summary>
/// Base mapping for entities deriving from <see cref="AppBaseEntity"/> that also
/// expose the CreatedAt / UpdatedAt audit columns on their table. Maps the Guid
/// primary key (GuidComb), optimistic-lock Version strategy and the two date
/// columns.
/// </summary>
/// <typeparam name="T">A concrete entity type deriving from AppBaseEntity.</typeparam>
public class EntityWithDatesMapping<T> : ClassMap<T> where T : AppBaseEntity
{
    public EntityWithDatesMapping()
    {
        base.Cache.NonStrictReadWrite();

        Id(e => e.Id).GeneratedBy.GuidComb();

        Map(e => e.CreatedAt).Not.Nullable().Default("CURRENT_TIMESTAMP");
        Map(e => e.UpdatedAt).Nullable();

        OptimisticLock.Version().DynamicUpdate();
    }
}