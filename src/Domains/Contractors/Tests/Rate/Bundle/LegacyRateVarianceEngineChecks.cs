namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LegacyRateVarianceEngineChecks
{
    public static void LegacyRateVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyRateVarianceEngine.DefaultLegacyRateVarianceEngine(Flags.NewRegistry());
        var item = LegacyRateVariance.NewLegacyRateVariance("code-635", "name-336");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyRateVarianceEngineKeepsResultsWithinBounds();
    }
}
