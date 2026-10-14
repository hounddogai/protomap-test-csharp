namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceVarianceViewChecks
{
    public static void BalanceVarianceViewShowsTheStatusInCapitals()
    {
        var item = BalanceVariance.NewBalanceVariance("code-848", "name-2");
        var view = BalanceVarianceView.ToBalanceVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceVarianceView.ToBalanceVarianceViews(new List<BalanceVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceVarianceViewShowsTheStatusInCapitals();
    }
}
