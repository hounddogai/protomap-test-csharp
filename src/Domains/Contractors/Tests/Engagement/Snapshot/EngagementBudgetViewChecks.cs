namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementBudgetViewChecks
{
    public static void EngagementBudgetViewShowsTheStatusInCapitals()
    {
        var item = EngagementBudget.NewEngagementBudget("code-954", "title-821");
        var view = EngagementBudgetView.ToEngagementBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementBudgetView.ToEngagementBudgetViews(new List<EngagementBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementBudgetViewShowsTheStatusInCapitals();
    }
}
