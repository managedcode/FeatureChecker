using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using ManagedCode.FeatureChecker.Definitions;

namespace ManagedCode.FeatureChecker.Targeting;

internal static class FeatureRollout
{
    private const double MaximumPercentage = 100d;
    private const int StackBufferLimit = 256;
    private const byte RolloutSeparator = (byte)':';

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

    public static int SelectVariantIndex(string featureKey, string targetingKey, IReadOnlyList<FeatureVariant> variants)
    {
        var totalWeight = 0d;
        var largestWeight = 0d;
        for (var index = 0; index < variants.Count; index++)
        {
            var weight = GetWeight(variants[index]);
            totalWeight += weight;
            largestWeight = Math.Max(largestWeight, weight);
        }

        if (totalWeight == 0d)
        {
            return 0;
        }

        var scale = 1d;
        if (double.IsPositiveInfinity(totalWeight))
        {
            scale = largestWeight;
            totalWeight = 0d;
            for (var index = 0; index < variants.Count; index++)
            {
                totalWeight += GetWeight(variants[index]) / scale;
            }
        }

        var bucket = GetBucket(featureKey, targetingKey) / MaximumPercentage * totalWeight;
        return SelectByBucket(variants, bucket, scale);
    }

    private static int SelectByBucket(IReadOnlyList<FeatureVariant> variants, double bucket, double scale)
    {
        var cursor = 0d;
        var lastWeightedIndex = 0;

        for (var index = 0; index < variants.Count; index++)
        {
            var weight = GetWeight(variants[index]);
            if (weight == 0d)
            {
                continue;
            }

            lastWeightedIndex = index;
            cursor += weight / scale;
            if (bucket < cursor)
            {
                return index;
            }
        }

        return lastWeightedIndex;
    }

    private static double GetWeight(FeatureVariant variant)
    {
        return double.IsFinite(variant.Weight) && variant.Weight > 0d ? variant.Weight : 0d;
    }

    private static double GetBucket(string featureKey, string targetingKey)
    {
        var encoding = Encoding.UTF8;
        var featureByteCount = encoding.GetByteCount(featureKey);
        var length = checked(featureByteCount + 1 + encoding.GetByteCount(targetingKey));
        Span<byte> input = length <= StackBufferLimit ? stackalloc byte[length] : new byte[length];
        encoding.GetBytes(featureKey, input[..featureByteCount]);
        input[featureByteCount] = RolloutSeparator;
        encoding.GetBytes(targetingKey, input[(featureByteCount + 1)..]);

        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(input, hash);
        var value = BinaryPrimitives.ReadUInt64BigEndian(hash);

        return value / (double)ulong.MaxValue * MaximumPercentage;
    }

}
