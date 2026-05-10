using System.Globalization;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed class FeatureEvaluationContextBuilder
{
    private readonly Dictionary<string, string> _attributes = new(StringComparer.OrdinalIgnoreCase);
    private string? _targetingKey;
    private string _contextKind = FeatureContextKinds.User;

    public FeatureEvaluationContextBuilder()
    {
    }

    public FeatureEvaluationContextBuilder(FeatureEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _targetingKey = context.TargetingKey;
        _contextKind = context.ContextKind;

        foreach (var (name, value) in context.Attributes)
        {
            _attributes[name] = value;
        }
    }

    public static FeatureEvaluationContextBuilder Create()
    {
        return new FeatureEvaluationContextBuilder();
    }

    public FeatureEvaluationContextBuilder WithTargetingKey(string targetingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetingKey);

        _targetingKey = targetingKey;

        return this;
    }

    public FeatureEvaluationContextBuilder WithContextKind(string contextKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextKind);

        _contextKind = contextKind;

        return this;
    }

    public FeatureEvaluationContextBuilder ForUser(string userId)
    {
        return WithContextKind(FeatureContextKinds.User).WithTargetingKey(userId).WithUserId(userId);
    }

    public FeatureEvaluationContextBuilder ForTenant(string tenantId)
    {
        return WithContextKind(FeatureContextKinds.Tenant).WithTargetingKey(tenantId).WithTenantId(tenantId);
    }

    public FeatureEvaluationContextBuilder ForSession(string sessionId)
    {
        return WithContextKind(FeatureContextKinds.Session).WithTargetingKey(sessionId).WithSessionId(sessionId);
    }

    public FeatureEvaluationContextBuilder WithUserId(string userId)
    {
        return With(FeatureContextAttributeNames.UserId, userId);
    }

    public FeatureEvaluationContextBuilder WithTenantId(string tenantId)
    {
        return With(FeatureContextAttributeNames.TenantId, tenantId);
    }

    public FeatureEvaluationContextBuilder WithSessionId(string sessionId)
    {
        return With(FeatureContextAttributeNames.SessionId, sessionId);
    }

    public FeatureEvaluationContextBuilder WithRole(string role)
    {
        return With(FeatureContextAttributeNames.Role, role);
    }

    public FeatureEvaluationContextBuilder WithPlan(string plan)
    {
        return With(FeatureContextAttributeNames.Plan, plan);
    }

    public FeatureEvaluationContextBuilder WithRegion(string region)
    {
        return With(FeatureContextAttributeNames.Region, region);
    }

    public FeatureEvaluationContextBuilder WithGroup(string group)
    {
        return With(FeatureContextAttributeNames.Group, group);
    }

    public FeatureEvaluationContextBuilder WithCountry(string country)
    {
        return With(FeatureContextAttributeNames.Country, country);
    }

    public FeatureEvaluationContextBuilder WithEmail(string email)
    {
        return With(FeatureContextAttributeNames.Email, email);
    }

    public FeatureEvaluationContextBuilder WithEnvironment(string environment)
    {
        return With(FeatureContextAttributeNames.Environment, environment);
    }

    public FeatureEvaluationContextBuilder WithApplication(string applicationId, string version)
    {
        return With(FeatureContextAttributeNames.ApplicationId, applicationId)
            .With(FeatureContextAttributeNames.ApplicationVersion, version);
    }

    public FeatureEvaluationContextBuilder WithDevice(string deviceId, string platform)
    {
        return With(FeatureContextAttributeNames.DeviceId, deviceId)
            .With(FeatureContextAttributeNames.DevicePlatform, platform);
    }

    public FeatureEvaluationContextBuilder With(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _attributes[name] = ConvertAttributeValue(value);

        return this;
    }

    public FeatureEvaluationContext Build()
    {
        return new FeatureEvaluationContext(_targetingKey, _attributes, _contextKind);
    }

    private static string ConvertAttributeValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string text => text,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
