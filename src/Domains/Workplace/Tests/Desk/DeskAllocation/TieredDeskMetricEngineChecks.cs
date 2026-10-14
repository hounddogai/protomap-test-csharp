namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class TieredDeskMetricEngineChecks
{
    public static void TieredDeskMetricEngineKeepsResultsWithinBounds()
    {
        var engine = TieredDeskMetricEngine.DefaultTieredDeskMetricEngine(Flags.NewRegistry());
        var item = TieredDeskMetric.NewTieredDeskMetric("code-127", "name-576");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredDeskMetricEngineKeepsResultsWithinBounds();
    }
}
