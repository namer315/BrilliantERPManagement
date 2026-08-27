using AutoMapper;
using CommonData.VO;
using System;
using System.Collections.Generic;
using System.Text;
using WhatsAppData.DTO;

namespace WhatsAppData.Extensions;

public static class ExtensionDTO
{
    /// <summary>
    /// Maps an object to a new instance of type TDestination.
    /// </summary>
    //public static TDestination MapTo<TDestination>(this object source)
    //{
    //    if (source == null) return default;
    //    return Mapper.Map<TDestination>(source);
    //}

    /// <summary>
    /// Maps the source object onto an existing destination object instance.
    /// </summary>
    //public static TDestination MapTo<TSource, TDestination>(this TSource source , TDestination destination)
    //{
    //    if (source == null) return destination;
    //    return Mapper.Map(source , destination);
    //}

    //public static TDestination MapChecked<TDestination>(this IMapper mapper , object source)
    //{
    //    TDestination destination = mapper.Map<TDestination>(source);

    //    if (destination is null)
    //        throw new NullReferenceException($"Mapping failed for type {typeof(TDestination).Name} from source {source.GetType().Name}");

    //    return destination;
    //}
    /// <summary>
    /// Maps an object to a new instance of type TDestination.
    /// </summary>
    public static TDestination MapTo<TDestination>(this EntityBase source)
    {
        if (source == null) return default;

        TDestination destination = DTOHelper.mapper.Map<TDestination>(source);

        if (destination is null)
            throw new NullReferenceException($"Mapping failed for type {typeof(TDestination).Name} from source {source.GetType().Name}");

        return destination;
    }
}
