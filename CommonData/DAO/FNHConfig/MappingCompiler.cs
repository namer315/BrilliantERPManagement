using CommonData.Services;
using CommonData.VO;
using FluentNHibernate.Cfg;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
using System;

namespace CommonData.DAO.FNHConfig;


/// <summary>
/// Pre-compiles and caches FluentNHibernate mappings to improve startup performance.
/// This class scans the assembly once and caches all mapping types, eliminating
/// the need for repeated reflection scans during SessionFactory initialization.
/// </summary>
public static class MappingCompiler
{
    private static readonly ConcurrentDictionary<Assembly , FluentNHibernate.PersistenceModel> _compiledModels
        = new ConcurrentDictionary<Assembly , FluentNHibernate.PersistenceModel>();

    private static readonly ConcurrentDictionary<Assembly , Type[]> _mappingTypesCache
        = new ConcurrentDictionary<Assembly , Type[]>();

    /// <summary>
    /// Gets or creates a pre-compiled PersistenceModel by auto-discovering mapping
    /// types across the CommonData assembly and other loaded assemblies that
    /// reference it (for example modules that contain additional entity mappings).
    /// </summary>
    public static FluentNHibernate.PersistenceModel GetCompiledModel()
    {
        var coreAssembly = typeof(TenantVO).Assembly;
        var coreName = coreAssembly.GetName().Name;

        // Find assemblies loaded into the AppDomain that reference the core assembly
        var referencingAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a != null)
            .Where(a => a == coreAssembly || a.GetReferencedAssemblies().Any(r => r.Name == coreName))
            .Distinct()
            .ToArray();

        // If only the core assembly is present, use the cached single-assembly model
        if (referencingAssemblies.Length == 1 && referencingAssemblies[0] == coreAssembly)
            return GetCompiledModel(coreAssembly);

        // Build and cache a merged model for the discovered assemblies. Cache key is based on core assembly
        // to preserve existing cache behavior while ensuring mappings are merged at runtime.
        return _compiledModels.GetOrAdd(coreAssembly, _ => BuildMergedModel(referencingAssemblies));
    }

    /// <summary>
    /// Gets or creates a pre-compiled PersistenceModel for the specified assembly.
    /// </summary>
    public static FluentNHibernate.PersistenceModel GetCompiledModel(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        return _compiledModels.GetOrAdd(assembly , CompileModelForAssembly);
    }

    private static FluentNHibernate.PersistenceModel BuildMergedModel(Assembly[] assemblies)
    {
        var model = new FluentNHibernate.PersistenceModel();

        foreach (var asm in assemblies)
        {
            try
            {
                var mappingTypes = GetMappingTypes(asm);
                foreach (var mappingType in mappingTypes)
                {
                    model.Add(mappingType);
                }
            }
            catch
            {
                // Ignore individual assembly failures — logging is handled in GetMappingTypes
            }
        }

        model.Conventions.Add(FluentNHibernate.Conventions.Helpers.DynamicUpdate.AlwaysTrue());
        model.Conventions.Add(FluentNHibernate.Conventions.Helpers.DynamicInsert.AlwaysTrue());

        return model;
    }

    /// <summary>
    /// Gets all mapping types from the assembly containing UserVO.
    /// </summary>
    public static Type[] GetMappingTypes()
    {
        return GetMappingTypes(typeof(TenantVO).Assembly);
    }

    /// <summary>
    /// Gets all mapping types from the specified assembly.
    /// </summary>
    public static Type[] GetMappingTypes(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        return _mappingTypesCache.GetOrAdd(assembly , FindMappingTypes);
    }

    private static FluentNHibernate.PersistenceModel CompileModelForAssembly(Assembly assembly)
    {
        var model = new FluentNHibernate.PersistenceModel();
        var mappingTypes = GetMappingTypes(assembly);

        foreach (var mappingType in mappingTypes)
        {
            model.Add(mappingType);
        }

        // Add the same conventions used in SessionFactoryGenerator
        model.Conventions.Add(FluentNHibernate.Conventions.Helpers.DynamicUpdate.AlwaysTrue());
        model.Conventions.Add(FluentNHibernate.Conventions.Helpers.DynamicInsert.AlwaysTrue());

        return model;
    }

    private static Type[] FindMappingTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes()
                .Where(t => typeof(FluentNHibernate.IMappingProvider).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            !t.IsInterface &&
                            !t.IsGenericTypeDefinition)
                .ToArray();
        }
        catch (Exception ex)
        {
            CurrentLogger.Instance?.Error(
                $"Failed to find mapping types in assembly {assembly.FullName}: {ex.Message}" , ex);
            return Array.Empty<Type>();
        }
    }

    /// <summary>
    /// Clears all cached models and mapping types.
    /// </summary>
    public static void ClearCache()
    {
        _compiledModels.Clear();
        _mappingTypesCache.Clear();
    }

    /// <summary>
    /// Gets the count of cached mapping types.
    /// </summary>
    public static int GetCachedMappingCount()
    {
        return GetCachedMappingCount(typeof(TenantVO).Assembly);
    }

    /// <summary>
    /// Gets the count of cached mapping types for an assembly.
    /// </summary>
    public static int GetCachedMappingCount(Assembly assembly)
    {
        if (_mappingTypesCache.TryGetValue(assembly , out var mappings))
            return mappings.Length;
        return -1;
    }
}

/// <summary>
/// Extension methods for easier usage of MappingCompiler with FluentNHibernate.
/// </summary>
public static class MappingCompilerExtensions
{
    /// <summary>
    /// Adds pre-compiled mappings from the UserVO assembly to the mapping configuration.
    /// </summary>
    /// <param name="mappingConfiguration">The mapping configuration.</param>
    /// <returns>The mapping configuration for chaining.</returns>
    public static MappingConfiguration AddPrecompiledMappings(
        this MappingConfiguration mappingConfiguration)
    {
        if (mappingConfiguration == null)
            throw new ArgumentNullException(nameof(mappingConfiguration));

        mappingConfiguration.UsePersistenceModel(MappingCompiler.GetCompiledModel());

        return mappingConfiguration;
    }

    /// <summary>
    /// Uses a pre-compiled PersistenceModel for the UserVO assembly.
    /// </summary>
    /// <param name="mappingConfiguration">The mapping configuration.</param>
    /// <returns>The mapping configuration for chaining.</returns>
    public static MappingConfiguration UsePrecompiledModel(
        this MappingConfiguration mappingConfiguration)
    {
        if (mappingConfiguration == null)
            throw new ArgumentNullException(nameof(mappingConfiguration));

        mappingConfiguration.UsePersistenceModel(MappingCompiler.GetCompiledModel());

        return mappingConfiguration;
    }
}