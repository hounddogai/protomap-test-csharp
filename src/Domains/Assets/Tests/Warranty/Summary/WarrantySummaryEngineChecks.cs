namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantySummaryEngineChecks
{
    public static void WarrantySummaryEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantySummaryEngine.DefaultWarrantySummaryEngine();
        var item = WarrantySummary.NewWarrantySummary("code-295", "label-67");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantySummaryEngineKeepsResultsWithinBounds();
    }
}
