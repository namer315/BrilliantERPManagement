using System;
using System.Collections.Generic;
using System.Text;

namespace CommonData.Extensions;

public static class Extension
{
    /// <summary>
    /// Converts a string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object.
    /// </summary>
    /// <typeparam name="TEnum">The target enum type.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <param name="ignoreCase">Whether to ignore case during conversion (default: true).</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when value cannot be parsed to TEnum.</exception>
    public static TEnum ToEnum<TEnum>(this string value , bool ignoreCase = true) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty." , nameof(value));
        }

        if (Enum.TryParse<TEnum>(value , ignoreCase , out var result))
        {
            return result;
        }

        throw new ArgumentException(
            $"'{value}' is not a valid value for enum type '{typeof(TEnum).Name}'. " +
            $"Valid options are: {string.Join(", " , Enum.GetNames(typeof(TEnum)))}"
        );
    }
}
