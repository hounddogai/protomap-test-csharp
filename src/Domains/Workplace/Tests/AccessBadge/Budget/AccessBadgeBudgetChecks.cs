namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeBudgetChecks
{
    public static void AccessBadgeBudgetStartsInOpen()
    {
        var item = AccessBadgeBudget.NewAccessBadgeBudget("code-754", "name-141");
        Check.Equal(item.Status, AccessBadgeBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeBudgetMovesToInReview()
    {
        var item = AccessBadgeBudget.NewAccessBadgeBudget("code-754", "name-141");
        try
        {
            item.MoveTo(AccessBadgeBudgetStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeBudgetStatus.InReview, "status");
    }

    public static void AccessBadgeBudgetRejectsMovesFromArchived()
    {
        var item = AccessBadgeBudget.NewAccessBadgeBudget("code-754", "name-141");
        item.Status = AccessBadgeBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(AccessBadgeBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccessBadgeBudgetStartsInOpen();
        AccessBadgeBudgetMovesToInReview();
        AccessBadgeBudgetRejectsMovesFromArchived();
    }
}
