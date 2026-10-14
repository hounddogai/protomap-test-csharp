namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredCostModelSummaryEngineChecks
{
    public static void TieredCostModelSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = TieredCostModelSummaryEngine.DefaultTieredCostModelSummaryEngine(Flags.NewRegistry());
        var item = TieredCostModelSummary.NewTieredCostModelSummary("code-351", "title-199");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredCostModelSummaryEngineKeepsResultsWithinBounds();
    }
}
