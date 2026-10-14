namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceMilestoneEngineChecks
{
    public static void BalanceMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceMilestoneEngine.DefaultBalanceMilestoneEngine(Flags.NewRegistry());
        var item = BalanceMilestone.NewBalanceMilestone("code-126", "label-713");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceMilestoneEngineKeepsResultsWithinBounds();
    }
}
