namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LegacyPoolVarianceEngineChecks
{
    public static void LegacyPoolVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPoolVarianceEngine.DefaultLegacyPoolVarianceEngine();
        var item = LegacyPoolVariance.NewLegacyPoolVariance("code-846", "title-32");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPoolVarianceEngineKeepsResultsWithinBounds();
    }
}
