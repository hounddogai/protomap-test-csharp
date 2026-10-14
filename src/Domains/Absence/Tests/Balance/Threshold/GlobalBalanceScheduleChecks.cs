namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class GlobalBalanceScheduleChecks
{
    public static void GlobalBalanceScheduleStartsInProposed()
    {
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("code-76", "title-282");
        Check.Equal(item.Status, GlobalBalanceScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalBalanceScheduleMovesToRejected()
    {
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("code-76", "title-282");
        try
        {
            item.MoveTo(GlobalBalanceScheduleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalBalanceScheduleStatus.Rejected, "status");
    }

    public static void GlobalBalanceScheduleRejectsMovesFromWithdrawn()
    {
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("code-76", "title-282");
        item.Status = GlobalBalanceScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalBalanceScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalBalanceScheduleStartsInProposed();
        GlobalBalanceScheduleMovesToRejected();
        GlobalBalanceScheduleRejectsMovesFromWithdrawn();
    }
}
