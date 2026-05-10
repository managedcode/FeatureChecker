using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Storage;

namespace ManagedCode.FeatureChecker.Definitions;

public interface IFeatureSetBuilder
{
    IFeatureDefinitionBuilder Feature(string key);

    FeatureSegmentBuilder Segment(string key);

    FeatureSnapshot Build();

    IFeatureEvaluator ToChecker();
}
