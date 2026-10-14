namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class FallbackCareerPathAdjustmentEngineChecks
{
    public static void FallbackCareerPathAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackCareerPathAdjustmentEngine.DefaultFallbackCareerPathAdjustmentEngine();
        var item = FallbackCareerPathAdjustment.NewFallbackCareerPathAdjustment("code-826", "label-343");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackCareerPathAdjustmentEngineKeepsResultsWithinBounds();
    }
}
