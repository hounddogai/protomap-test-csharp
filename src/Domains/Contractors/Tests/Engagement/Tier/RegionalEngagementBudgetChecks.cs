namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalEngagementBudgetChecks
{
    public static void RegionalEngagementBudgetStartsInRequested()
    {
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("code-227", "label-259");
        Check.Equal(item.Status, RegionalEngagementBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalEngagementBudgetMovesToGranted()
    {
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("code-227", "label-259");
        try
        {
            item.MoveTo(RegionalEngagementBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalEngagementBudgetStatus.Granted, "status");
    }

    public static void RegionalEngagementBudgetRejectsMovesFromExpired()
    {
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("code-227", "label-259");
        item.Status = RegionalEngagementBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalEngagementBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalEngagementBudgetStartsInRequested();
        RegionalEngagementBudgetMovesToGranted();
        RegionalEngagementBudgetRejectsMovesFromExpired();
    }
}
