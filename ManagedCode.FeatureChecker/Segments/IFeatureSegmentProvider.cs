namespace ManagedCode.FeatureChecker.Segments;

public interface IFeatureSegmentProvider
{
    IReadOnlyCollection<FeatureSegment> GetFeatureSegments();
}
