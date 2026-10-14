namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceSummaryViewChecks
{
    public static void BalanceSummaryViewShowsTheStatusInCapitals()
    {
        var item = BalanceSummary.NewBalanceSummary("code-165", "name-165");
        var view = BalanceSummaryView.ToBalanceSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceSummaryView.ToBalanceSummaryViews(new List<BalanceSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceSummaryViewShowsTheStatusInCapitals();
    }
}
