namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantLedger;

using Acme.Workforce.Compensation.EquityGrant.GrantLedger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of grant ledger code, run by the project's runner.</summary>
public static class StrictEquityGrantMilestoneEngineChecks
{
    public static void StrictEquityGrantMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = StrictEquityGrantMilestoneEngine.DefaultStrictEquityGrantMilestoneEngine(Flags.NewRegistry());
        var item = StrictEquityGrantMilestone.NewStrictEquityGrantMilestone("code-211", "title-786");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictEquityGrantMilestoneEngineKeepsResultsWithinBounds();
    }
}
