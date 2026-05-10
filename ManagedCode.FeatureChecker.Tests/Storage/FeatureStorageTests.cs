using System.Text;

namespace ManagedCode.FeatureChecker.Tests.Storage;

public sealed class FeatureStorageTests
{
    [Test]
    public void FeatureSnapshotSerializer_ShouldRoundTripJsonAndStreams()
    {
        var snapshot = new FeatureSnapshot
        {
            Features =
            [
                new FeatureDefinition
                {
                    Key = FeatureNames.MarketplaceConnect,
                    Status = FeatureStatus.Enabled,
                    Description = Descriptions.Marketplace,
                    Value = DefaultValues.EnabledValue
                }
            ]
        };

        var json = FeatureSnapshotSerializer.Serialize(snapshot);
        var fromJson = FeatureSnapshotSerializer.Deserialize(json);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var fromStream = FeatureSnapshotSerializer.Read(stream);

        fromJson.Features.Single().Key.ShouldBe(FeatureNames.MarketplaceConnect);
        fromStream.Features.Single().Value.ShouldBe(DefaultValues.EnabledValue);
    }

    [Test]
    public void FeatureSnapshotSerializer_ShouldLoadAndSaveFilesForStorageAdapters()
    {
        var snapshot = FeatureSnapshot.FromDefinitions(
            [
                FeatureDefinition.Create(FeatureNames.RetiredExport, FeatureStatus.Disabled)
            ]);
        var path = CreateTempJsonPath();

        try
        {
            FeatureSnapshotSerializer.Save(path, snapshot);
            var loaded = FeatureSnapshotSerializer.Load(path);
            var checker = new FeatureCheckerEvaluator(new FeatureFileProvider(path));

            loaded.Features.Single().Status.ShouldBe(FeatureStatus.Disabled);
            checker.IsDisabled(FeatureNames.RetiredExport).ShouldBeTrue();
        }
        finally
        {
            DeleteFile(path);
        }
    }

    [Test]
    public void FeatureFileProvider_ShouldLoadFullSnapshotWithSegments()
    {
        var snapshot = new FeatureSnapshot
        {
            Features =
            [
                new FeatureDefinition
                {
                    Key = FeatureNames.MarketplaceConnect,
                    Status = FeatureStatus.Disabled,
                    Rules =
                    [
                        new FeatureTargetingRule
                        {
                            IncludeSegments = [SegmentNames.EnterpriseUsers],
                            Status = FeatureStatus.Enabled
                        }
                    ]
                }
            ],
            Segments =
            [
                new FeatureSegment
                {
                    Key = SegmentNames.EnterpriseUsers,
                    IncludedKeys = [TargetingKeys.UserA]
                }
            ]
        };
        var path = CreateTempJsonPath();

        try
        {
            FeatureSnapshotSerializer.Save(path, snapshot);
            var checker = new FeatureCheckerEvaluator(new FeatureFileProvider(path));
            var context = FeatureEvaluationContextBuilder.Create().ForUser(TargetingKeys.UserA).Build();

            checker.IsEnabled(FeatureNames.MarketplaceConnect, context).ShouldBeTrue();
        }
        finally
        {
            DeleteFile(path);
        }
    }

    private static string CreateTempJsonPath()
    {
        return Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static class FeatureNames
    {
        public const string MarketplaceConnect = "marketplace.connect";
        public const string RetiredExport = "retired.export";
    }

    private static class SegmentNames
    {
        public const string EnterpriseUsers = "enterprise-users";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
    }

    private static class DefaultValues
    {
        public const string EnabledValue = "enabled-value";
    }

    private static class Descriptions
    {
        public const string Marketplace = "Marketplace integration feature";
    }
}
