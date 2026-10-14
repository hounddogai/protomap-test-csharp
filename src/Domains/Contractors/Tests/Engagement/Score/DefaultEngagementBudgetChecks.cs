namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class DefaultEngagementBudgetChecks
{
    public static void DefaultEngagementBudgetStartsInProposed()
    {
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("code-289", "label-724");
        Check.Equal(item.Status, DefaultEngagementBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultEngagementBudgetMovesToRejected()
    {
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("code-289", "label-724");
        try
        {
            item.MoveTo(DefaultEngagementBudgetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultEngagementBudgetStatus.Rejected, "status");
    }

    public static void DefaultEngagementBudgetRejectsMovesFromWithdrawn()
    {
        var item = DefaultEngagementBudget.NewDefaultEngagementBudget("code-289", "label-724");
        item.Status = DefaultEngagementBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultEngagementBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultEngagementBudgetStartsInProposed();
        DefaultEngagementBudgetMovesToRejected();
        DefaultEngagementBudgetRejectsMovesFromWithdrawn();
    }
}
