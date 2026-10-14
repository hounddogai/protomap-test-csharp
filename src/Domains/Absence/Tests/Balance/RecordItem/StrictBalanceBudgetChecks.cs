namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictBalanceBudgetChecks
{
    public static void StrictBalanceBudgetStartsInPlanned()
    {
        var item = StrictBalanceBudget.NewStrictBalanceBudget("code-116", "label-805");
        Check.Equal(item.Status, StrictBalanceBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictBalanceBudgetMovesToScheduled()
    {
        var item = StrictBalanceBudget.NewStrictBalanceBudget("code-116", "label-805");
        try
        {
            item.MoveTo(StrictBalanceBudgetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictBalanceBudgetStatus.Scheduled, "status");
    }

    public static void StrictBalanceBudgetRejectsMovesFromCancelled()
    {
        var item = StrictBalanceBudget.NewStrictBalanceBudget("code-116", "label-805");
        item.Status = StrictBalanceBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictBalanceBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictBalanceBudgetStartsInPlanned();
        StrictBalanceBudgetMovesToScheduled();
        StrictBalanceBudgetRejectsMovesFromCancelled();
    }
}
