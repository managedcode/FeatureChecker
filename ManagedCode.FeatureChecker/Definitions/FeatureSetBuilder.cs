using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Storage;
using FeatureCheckerEvaluator = ManagedCode.FeatureChecker.Evaluation.FeatureChecker;

namespace ManagedCode.FeatureChecker.Definitions;

public sealed class FeatureSetBuilder : IFeatureSetBuilder
{
    private readonly List<FeatureDefinitionBuilder> _features = [];
    private readonly List<FeatureSegmentBuilder> _segments = [];

    public FeatureDefinitionBuilder Feature(string key)
    {
        var builder = new FeatureDefinitionBuilder(key);
        _features.Add(builder);

        return builder;
    }

    public FeatureSegmentBuilder Segment(string key)
    {
        var builder = new FeatureSegmentBuilder(key);
        _segments.Add(builder);

        return builder;
    }

    public FeatureSnapshot Build()
    {
        return new FeatureSnapshot
        {
            Features = _features.ConvertAll(static feature => feature.Build()),
            Segments = _segments.ConvertAll(static segment => segment.Build())
        };
    }

    public FeatureCheckerEvaluator ToChecker()
    {
        return new FeatureCheckerEvaluator(Build());
    }

    IFeatureDefinitionBuilder IFeatureSetBuilder.Feature(string key)
    {
        return Feature(key);
    }

    IFeatureEvaluator IFeatureSetBuilder.ToChecker()
    {
        return ToChecker();
    }
}
