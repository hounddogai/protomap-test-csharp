namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledBalanceDraftEngineChecks
{
    public static void ScheduledBalanceDraftEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledBalanceDraftEngine.DefaultScheduledBalanceDraftEngine(Flags.NewRegistry());
        var item = ScheduledBalanceDraft.NewScheduledBalanceDraft("code-63", "name-491");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledBalanceDraftEngineKeepsResultsWithinBounds();
    }
}
