namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedBalanceScheduleChecks
{
    public static void WeightedBalanceScheduleStartsInDraft()
    {
        var item = WeightedBalanceSchedule.NewWeightedBalanceSchedule("code-502", "title-500");
        Check.Equal(item.Status, WeightedBalanceScheduleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedBalanceScheduleMovesToPending()
    {
        var item = WeightedBalanceSchedule.NewWeightedBalanceSchedule("code-502", "title-500");
        try
        {
            item.MoveTo(WeightedBalanceScheduleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedBalanceScheduleStatus.Pending, "status");
    }

    public static void WeightedBalanceScheduleRejectsMovesFromClosed()
    {
        var item = WeightedBalanceSchedule.NewWeightedBalanceSchedule("code-502", "title-500");
        item.Status = WeightedBalanceScheduleStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedBalanceScheduleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedBalanceScheduleStartsInDraft();
        WeightedBalanceScheduleMovesToPending();
        WeightedBalanceScheduleRejectsMovesFromClosed();
    }
}
