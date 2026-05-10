using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Access;

public interface IFeatureCheckerFactory
{
    IFeatureEvaluator Create();

    IFeatureScope CreateScope(FeatureEvaluationContext? context = null);
}
