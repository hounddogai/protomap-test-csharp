namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class WeightedPolicyOverrideEngineChecks
{
    public static void WeightedPolicyOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedPolicyOverrideEngine.DefaultWeightedPolicyOverrideEngine();
        var item = WeightedPolicyOverride.NewWeightedPolicyOverride("code-679", "name-782");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedPolicyOverrideEngineKeepsResultsWithinBounds();
    }
}
