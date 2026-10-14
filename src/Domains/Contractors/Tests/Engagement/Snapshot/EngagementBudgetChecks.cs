namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementBudgetChecks
{
    public static void EngagementBudgetStartsInOpen()
    {
        var item = EngagementBudget.NewEngagementBudget("code-954", "title-821");
        Check.Equal(item.Status, EngagementBudgetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementBudgetMovesToResolved()
    {
        var item = EngagementBudget.NewEngagementBudget("code-954", "title-821");
        try
        {
            item.MoveTo(EngagementBudgetStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementBudgetStatus.Resolved, "status");
    }

    public static void EngagementBudgetRejectsMovesFromArchived()
    {
        var item = EngagementBudget.NewEngagementBudget("code-954", "title-821");
        item.Status = EngagementBudgetStatus.Archived;
        Check.Fails(() => item.MoveTo(EngagementBudgetStatus.Open), "move");
    }

    public static void RunAll()
    {
        EngagementBudgetStartsInOpen();
        EngagementBudgetMovesToResolved();
        EngagementBudgetRejectsMovesFromArchived();
    }
}
