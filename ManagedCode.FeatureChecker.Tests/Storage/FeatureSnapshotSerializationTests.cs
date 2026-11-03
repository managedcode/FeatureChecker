using System.Text;
using System.Text.Json;

namespace ManagedCode.FeatureChecker.Tests.Storage;

public sealed class FeatureSnapshotSerializationTests
{
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
