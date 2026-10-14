namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledRetroPayEntryChecks
{
    public static void ScheduledRetroPayEntryStartsInProposed()
    {
        var item = ScheduledRetroPayEntry.NewScheduledRetroPayEntry("code-134", "title-273");
        Check.Equal(item.Status, ScheduledRetroPayEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledRetroPayEntryMovesToApproved()
    {
        var item = ScheduledRetroPayEntry.NewScheduledRetroPayEntry("code-134", "title-273");
        try
        {
            item.MoveTo(ScheduledRetroPayEntryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledRetroPayEntryStatus.Approved, "status");
    }

    public static void ScheduledRetroPayEntryRejectsMovesFromWithdrawn()
    {
        var item = ScheduledRetroPayEntry.NewScheduledRetroPayEntry("code-134", "title-273");
        item.Status = ScheduledRetroPayEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledRetroPayEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledRetroPayEntryStartsInProposed();
        ScheduledRetroPayEntryMovesToApproved();
        ScheduledRetroPayEntryRejectsMovesFromWithdrawn();
    }
}
