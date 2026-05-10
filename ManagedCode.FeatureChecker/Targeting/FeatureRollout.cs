using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using ManagedCode.FeatureChecker.Definitions;

namespace ManagedCode.FeatureChecker.Targeting;

internal static class FeatureRollout
{
    private const double MaximumPercentage = 100d;

    public static bool IsIncluded(string featureKey, string targetingKey, double percentage)
    {
        if (percentage <= 0d)
        {
            return false;
        }

        if (percentage >= MaximumPercentage)
        {
            return true;
        }

        return GetBucket(featureKey, targetingKey) < percentage;
    }

    public static FeatureVariant? SelectVariant(string featureKey, string targetingKey, IReadOnlyList<FeatureVariant> variants)
    {
        if (variants.Count == 0)
        {
            return null;
        }

        var weightedVariants = variants
            .Where(variant => variant.Weight > 0d)
            .ToArray();

        if (weightedVariants.Length == 0)
        {
            return variants[0];
        }

        var totalWeight = weightedVariants.Sum(variant => variant.Weight);
        var bucket = GetBucket(featureKey, targetingKey) / MaximumPercentage * totalWeight;
        var cursor = 0d;

        foreach (var variant in weightedVariants)
        {
            cursor += variant.Weight;

            if (bucket < cursor)
            {
                return variant;
            }
        }

        return weightedVariants[^1];
    }

    private static double GetBucket(string featureKey, string targetingKey)
    {
        var key = string.Concat(featureKey, Constants.RolloutSeparator, targetingKey);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var value = BinaryPrimitives.ReadUInt64BigEndian(hash);

        return value / (double)ulong.MaxValue * MaximumPercentage;
    }

    private static class Constants
    {
        public const string RolloutSeparator = ":";
    }
}
