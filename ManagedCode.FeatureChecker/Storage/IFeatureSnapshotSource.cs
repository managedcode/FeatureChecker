namespace ManagedCode.FeatureChecker.Storage;

public interface IFeatureSnapshotSource
{
    FeatureSnapshot GetSnapshot();
}
