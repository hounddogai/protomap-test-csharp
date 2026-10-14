namespace Acme.Workforce.Payrollops.Tests.PayRun.RunVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>Checks of run variance code, run by the project's runner.</summary>
public static class TieredPayRunMilestoneEngineChecks
{
    public static void TieredPayRunMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPayRunMilestoneEngine.DefaultTieredPayRunMilestoneEngine(Flags.NewRegistry());
        var item = TieredPayRunMilestone.NewTieredPayRunMilestone("code-648", "label-331");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPayRunMilestoneEngineKeepsResultsWithinBounds();
    }
}
