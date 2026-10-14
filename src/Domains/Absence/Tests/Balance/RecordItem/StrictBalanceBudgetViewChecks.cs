namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictBalanceBudgetViewChecks
{
    public static void StrictBalanceBudgetViewShowsTheStatusInCapitals()
    {
        var item = StrictBalanceBudget.NewStrictBalanceBudget("code-116", "label-805");
        var view = StrictBalanceBudgetView.ToStrictBalanceBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictBalanceBudgetView.ToStrictBalanceBudgetViews(new List<StrictBalanceBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictBalanceBudgetViewShowsTheStatusInCapitals();
    }
}
