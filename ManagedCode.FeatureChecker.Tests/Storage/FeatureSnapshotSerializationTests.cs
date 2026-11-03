using System.Text;
using System.Text.Json;

namespace ManagedCode.FeatureChecker.Tests.Storage;

public sealed class FeatureSnapshotSerializationTests
{
    [Test]
    [Arguments("status")]
    [Arguments("Status")]
    public void DuplicateFeatureStatus_IsRejectedByAllSnapshotReaders(string duplicateProperty)
    {
        var json = $$"""{"features":[{"key":"checkout","status":"Disabled","{{duplicateProperty}}":"Enabled"}]}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var path = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.json");
        try
        {
            File.WriteAllText(path, json, Encoding.UTF8);

            Should.Throw<JsonException>(() => FeatureSnapshotSerializer.Deserialize(json));
            Should.Throw<JsonException>(() => FeatureSnapshotSerializer.Read(stream));
            Should.Throw<JsonException>(() => FeatureSnapshotSerializer.Load(path));
            Should.Throw<JsonException>(() => new FeatureFileProvider(path).GetSnapshot());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Test]
    public void Save_LeavesReadOnlySnapshotUntouched()
    {
        const string key = "checkout";
        var path = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.json");
        var original = FeatureSnapshot.FromDefinitions([FeatureDefinition.Create(key, FeatureStatus.Enabled)]);
        var replacement = FeatureSnapshot.FromDefinitions([FeatureDefinition.Create(key, FeatureStatus.Disabled)]);
        FeatureSnapshotSerializer.Save(path, original);
        var attributes = File.GetAttributes(path);
        try
        {
            File.SetAttributes(path, attributes | FileAttributes.ReadOnly);

            Should.Throw<UnauthorizedAccessException>(() => FeatureSnapshotSerializer.Save(path, replacement));

            FeatureSnapshotSerializer.Load(path).Features.Single().Status.ShouldBe(FeatureStatus.Enabled);
            File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly).ShouldBeTrue();
        }
        finally
        {
            File.SetAttributes(path, attributes);
            File.Delete(path);
        }
    }

    [Test]
    public void NullJson_IsRejectedByStringAndStreamReaders()
    {
        using var stream = new MemoryStream("null"u8.ToArray());

        Should.Throw<JsonException>(() => FeatureSnapshotSerializer.Deserialize("null"));
        Should.Throw<JsonException>(() => FeatureSnapshotSerializer.Read(stream));
    }

    [Test]
    public void Save_PreservesThePreviousSnapshotWhenSerializationFails()
    {
        const string key = "checkout";
        var path = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.json");
        try
        {
            var snapshot = FeatureSnapshot.FromDefinitions([FeatureDefinition.Create(key, FeatureStatus.Enabled)]);
            var builder = new FeatureSetBuilder();
            builder.Feature(key).Variant("invalid", double.NaN);
            FeatureSnapshotSerializer.Save(path, snapshot);
            var original = File.ReadAllText(path, Encoding.UTF8);

            Should.Throw<ArgumentException>(() => FeatureSnapshotSerializer.Save(path, builder.Build()));

            File.ReadAllText(path, Encoding.UTF8).ShouldBe(original);
            FeatureSnapshotSerializer.Load(path).Features.Single().Status.ShouldBe(FeatureStatus.Enabled);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
