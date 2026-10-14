namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingReturnPlanRollupEngineChecks
{
    public static void PendingReturnPlanRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PendingReturnPlanRollupEngine.DefaultPendingReturnPlanRollupEngine();
        var item = PendingReturnPlanRollup.NewPendingReturnPlanRollup("code-562", "title-355");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingReturnPlanRollupEngineKeepsResultsWithinBounds();
    }
}
