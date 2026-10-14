namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ManualReorgMetricEngineChecks
{
    public static void ManualReorgMetricEngineKeepsResultsWithinBounds()
    {
        var engine = ManualReorgMetricEngine.DefaultManualReorgMetricEngine(Flags.NewRegistry());
        var item = ManualReorgMetric.NewManualReorgMetric("code-963", "title-168");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualReorgMetricEngineKeepsResultsWithinBounds();
    }
}
