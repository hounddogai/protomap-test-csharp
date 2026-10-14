namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingHeadcountBudgetChecks
{
    public static void PendingHeadcountBudgetStartsInOpen()
    {
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("code-545", "title-595");
        Check.Equal(item.Status, PendingHeadcountBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingHeadcountBudgetMovesToResolved()
    {
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("code-545", "title-595");
        try
        {
            item.MoveTo(PendingHeadcountBudgetStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingHeadcountBudgetStatus.Resolved, "status");
    }

    public static void PendingHeadcountBudgetRejectsMovesFromArchived()
    {
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("code-545", "title-595");
        item.Status = PendingHeadcountBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingHeadcountBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingHeadcountBudgetStartsInOpen();
        PendingHeadcountBudgetMovesToResolved();
        PendingHeadcountBudgetRejectsMovesFromArchived();
    }
}
