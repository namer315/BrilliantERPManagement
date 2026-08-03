using NHibernate.Criterion;

namespace CommonData.Search;

/// <summary>
/// Fluent query builder for TenantVO search filters.
/// Composable: multiple filters chain together.
/// </summary>
public class TenantSearch
{
    private readonly List<Action<NHibernate.IQueryOver<VO.TenantVO, VO.TenantVO>>> _filters = [];

    /// <summary>
    /// Filter by exact name match.
    /// </summary>
    public TenantSearch ByName(string name)
    {
        _filters.Add(q => q.Where(t => t.Name == name));
        return this;
    }

    /// <summary>
    /// Filter by partial name match (case-insensitive LIKE).
    /// </summary>
    public TenantSearch ByNameLike(string pattern)
    {
        _filters.Add(q => q.WhereRestrictionOn(t => t.Name)
            .IsInsensitiveLike(pattern, MatchMode.Anywhere));
        return this;
    }

    /// <summary>
    /// Filter by exact token match.
    /// </summary>
    public TenantSearch ByToken(string token)
    {
        _filters.Add(q => q.Where(t => t.Token == token));
        return this;
    }

    /// <summary>
    /// Exclude soft-deleted records from results.
    /// </summary>
    public TenantSearch ActiveOnly()
    {
        _filters.Add(q => q.Where(t => t.Active));
        return this;
    }

    /// <summary>
    /// Include soft-deleted records (overrides the global SoftDelete filter temporarily).
    /// </summary>
    public TenantSearch IncludeDeleted()
    {
        _filters.Add(q => q.Where(t => t.IsDeleted == true));
        return this;
    }

    /// <summary>
    /// Applies all accumulated filters to the given QueryOver.
    /// </summary>
    public NHibernate.IQueryOver<VO.TenantVO, VO.TenantVO> Apply(
        NHibernate.IQueryOver<VO.TenantVO, VO.TenantVO> query)
    {
        foreach (var filter in _filters)
        {
            filter(query);
        }
        return query;
    }

    /// <summary>
    /// True if any filters have been added.
    /// </summary>
    public bool HasFilters => _filters.Count > 0;
}
