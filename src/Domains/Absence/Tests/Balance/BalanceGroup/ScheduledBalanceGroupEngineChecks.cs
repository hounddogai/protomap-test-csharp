namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class ScheduledBalanceGroupEngineChecks
{
    public static void ScheduledBalanceGroupEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledBalanceGroupEngine.DefaultScheduledBalanceGroupEngine(Flags.NewRegistry());
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("code-125", "name-198");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledBalanceGroupEngineKeepsResultsWithinBounds();
    }
}
