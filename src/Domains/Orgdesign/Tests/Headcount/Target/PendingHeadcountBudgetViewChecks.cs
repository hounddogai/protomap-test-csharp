namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingHeadcountBudgetViewChecks
{
    public static void PendingHeadcountBudgetViewShowsTheStatusInCapitals()
    {
        var item = PendingHeadcountBudget.NewPendingHeadcountBudget("code-545", "title-595");
        var view = PendingHeadcountBudgetView.ToPendingHeadcountBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingHeadcountBudgetView.ToPendingHeadcountBudgetViews(new List<PendingHeadcountBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingHeadcountBudgetViewShowsTheStatusInCapitals();
    }
}
