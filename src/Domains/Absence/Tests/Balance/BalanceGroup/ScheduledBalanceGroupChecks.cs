namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class ScheduledBalanceGroupChecks
{
    public static void ScheduledBalanceGroupStartsInPlanned()
    {
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("code-125", "name-198");
        Check.Equal(item.Status, ScheduledBalanceGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledBalanceGroupMovesToScheduled()
    {
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("code-125", "name-198");
        try
        {
            item.MoveTo(ScheduledBalanceGroupStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledBalanceGroupStatus.Scheduled, "status");
    }

    public static void ScheduledBalanceGroupRejectsMovesFromCancelled()
    {
        var item = ScheduledBalanceGroup.NewScheduledBalanceGroup("code-125", "name-198");
        item.Status = ScheduledBalanceGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledBalanceGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledBalanceGroupStartsInPlanned();
        ScheduledBalanceGroupMovesToScheduled();
        ScheduledBalanceGroupRejectsMovesFromCancelled();
    }
}
