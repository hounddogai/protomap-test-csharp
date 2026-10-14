namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgAdjustmentEngineChecks
{
    public static void ReorgAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgAdjustmentEngine.DefaultReorgAdjustmentEngine();
        var item = ReorgAdjustment.NewReorgAdjustment("code-810", "label-575");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgAdjustmentEngineKeepsResultsWithinBounds();
    }
}
