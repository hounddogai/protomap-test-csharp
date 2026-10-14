namespace Acme.Workforce.Absence.Tests.ReturnPlan.Entry;

using Acme.Workforce.Absence.ReturnPlan.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalReturnPlanRollupEngineChecks
{
    public static void SeasonalReturnPlanRollupEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalReturnPlanRollupEngine.DefaultSeasonalReturnPlanRollupEngine(Flags.NewRegistry());
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("code-318", "name-180");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalReturnPlanRollupEngineKeepsResultsWithinBounds();
    }
}
