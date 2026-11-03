using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace ManagedCode.FeatureChecker;

internal static class FeatureCollections
{
    public static IReadOnlyDictionary<string, string> EmptyMetadata => FrozenDictionary<string, string>.Empty;

    public static IReadOnlyList<T> Freeze<T>(IReadOnlyList<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return values switch
        {
            { Count: 0 } => [],
            ImmutableArray<T> => values,
            _ => values.ToImmutableArray()
        };
    }

    public static IReadOnlyDictionary<string, string> FreezeMetadata(IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return values switch
        {
            FrozenDictionary<string, string> or ImmutableDictionary<string, string> or ImmutableSortedDictionary<string, string> => values,
            Dictionary<string, string> dictionary => dictionary.ToFrozenDictionary(dictionary.Comparer),
            ConcurrentDictionary<string, string> concurrent => concurrent.ToArray().ToFrozenDictionary(concurrent.Comparer),
            SortedDictionary<string, string> sorted => ImmutableSortedDictionary.CreateRange(sorted.Comparer, sorted),
            SortedList<string, string> sorted => ImmutableSortedDictionary.CreateRange(sorted.Comparer, sorted),
            _ => values.ToFrozenDictionary(StringComparer.Ordinal)
        };
    }
}
