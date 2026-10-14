namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class ScheduledHeadcountBudgetViewChecks
{
    public static void ScheduledHeadcountBudgetViewShowsTheStatusInCapitals()
    {
        var item = ScheduledHeadcountBudget.NewScheduledHeadcountBudget("code-530", "name-801");
        var view = ScheduledHeadcountBudgetView.ToScheduledHeadcountBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledHeadcountBudgetView.ToScheduledHeadcountBudgetViews(new List<ScheduledHeadcountBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledHeadcountBudgetViewShowsTheStatusInCapitals();
    }
}
