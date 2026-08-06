namespace CommonData.Session;

/// <summary>
/// Determines how the NHibernate schema is managed at application startup,
/// driven by the <c>NHibernate:SchemaMode</c> configuration key.
/// </summary>
public enum SchemaMode
{
    /// <summary>No schema operation. Use when the schema is managed externally or exists.</summary>
    None = 0,

    /// <summary>
    /// Destructive: drops existing tables/objects and recreates them from the mappings.
    /// DEV ONLY — will delete data.
    /// </summary>
    Create = 1,

    /// <summary>
    /// Non-destructive: adds/alters tables to match the mappings without dropping data.
    /// Recommended for incremental development against an existing database.
    /// </summary>
    Update = 2,

    /// <summary>Validates the existing schema against the mapping metadata; logs mismatches.</summary>
    Validate = 3,
}
