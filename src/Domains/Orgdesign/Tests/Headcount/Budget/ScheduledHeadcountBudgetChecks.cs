namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ScheduledHeadcountBudgetChecks
{
    public static void ScheduledHeadcountBudgetStartsInPlanned()
    {
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("code-530", "name-801");
        Check.Equal(item.Status, ScheduledHeadcountBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledHeadcountBudgetMovesToScheduled()
    {
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("code-530", "name-801");
        try
        {
            item.MoveTo(ScheduledHeadcountBudgetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledHeadcountBudgetStatus.Scheduled, "status");
    }

    public static void ScheduledHeadcountBudgetRejectsMovesFromCancelled()
    {
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("code-530", "name-801");
        item.Status = ScheduledHeadcountBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledHeadcountBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledHeadcountBudgetStartsInPlanned();
        ScheduledHeadcountBudgetMovesToScheduled();
        ScheduledHeadcountBudgetRejectsMovesFromCancelled();
    }
}
