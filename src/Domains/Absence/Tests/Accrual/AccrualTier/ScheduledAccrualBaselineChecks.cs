namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class ScheduledAccrualBaselineChecks
{
    public static void ScheduledAccrualBaselineStartsInIdle()
    {
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("code-755", "title-589");
        Check.Equal(item.Status, ScheduledAccrualBaselineStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAccrualBaselineMovesToDraining()
    {
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("code-755", "title-589");
        try
        {
            item.MoveTo(ScheduledAccrualBaselineStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAccrualBaselineStatus.Draining, "status");
    }

    public static void ScheduledAccrualBaselineRejectsMovesFromStopped()
    {
        var item = ScheduledAccrualBaseline.NewScheduledAccrualBaseline("code-755", "title-589");
        item.Status = ScheduledAccrualBaselineStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledAccrualBaselineStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledAccrualBaselineStartsInIdle();
        ScheduledAccrualBaselineMovesToDraining();
        ScheduledAccrualBaselineRejectsMovesFromStopped();
    }
}
