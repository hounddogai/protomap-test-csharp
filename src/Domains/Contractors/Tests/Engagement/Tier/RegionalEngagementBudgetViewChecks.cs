namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalEngagementBudgetViewChecks
{
    public static void RegionalEngagementBudgetViewShowsTheStatusInCapitals()
    {
        var item = RegionalEngagementBudget.NewRegionalEngagementBudget("code-227", "label-259");
        var view = RegionalEngagementBudgetView.ToRegionalEngagementBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalEngagementBudgetView.ToRegionalEngagementBudgetViews(new List<RegionalEngagementBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalEngagementBudgetViewShowsTheStatusInCapitals();
    }
}
