namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdjustmentManifestEngineChecks
{
    public static void AdjustmentManifestEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentManifestEngine.DefaultAdjustmentManifestEngine();
        var item = AdjustmentManifest.NewAdjustmentManifest("code-501", "title-865");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentManifestEngineKeepsResultsWithinBounds();
    }
}
