namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class ScheduledAccrualBaselineEngineChecks
{
    public static void ScheduledAccrualBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledAccrualBaselineEngine.DefaultScheduledAccrualBaselineEngine(Flags.NewRegistry());
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("code-755", "title-589");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledAccrualBaselineEngineKeepsResultsWithinBounds();
    }
}
