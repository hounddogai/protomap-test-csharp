namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyEngagementBudgetChecks
{
    public static void LegacyEngagementBudgetStartsInDraft()
    {
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("code-831", "name-592");
        Check.Equal(item.Status, LegacyEngagementBudgetStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyEngagementBudgetMovesToActive()
    {
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("code-831", "name-592");
        try
        {
            item.MoveTo(LegacyEngagementBudgetStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyEngagementBudgetStatus.Active, "status");
    }

    public static void LegacyEngagementBudgetRejectsMovesFromClosed()
    {
        var item = LegacyEngagementBudget.NewLegacyEngagementBudget("code-831", "name-592");
        item.Status = LegacyEngagementBudgetStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyEngagementBudgetStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyEngagementBudgetStartsInDraft();
        LegacyEngagementBudgetMovesToActive();
        LegacyEngagementBudgetRejectsMovesFromClosed();
    }
}
