namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BalanceRollupViewChecks
{
    public static void BalanceRollupViewShowsTheStatusInCapitals()
    {
        var item = BalanceRollup.NewBalanceRollup("code-201", "name-206");
        var view = BalanceRollupView.ToBalanceRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceRollupView.ToBalanceRollupViews(new List<BalanceRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceRollupViewShowsTheStatusInCapitals();
    }
}
