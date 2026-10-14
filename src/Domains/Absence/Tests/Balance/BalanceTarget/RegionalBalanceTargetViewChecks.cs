namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class RegionalBalanceTargetViewChecks
{
    public static void RegionalBalanceTargetViewShowsTheStatusInCapitals()
    {
        var item = RegionalBalanceTarget.NewRegionalBalanceTarget("code-502", "name-176");
        var view = RegionalBalanceTargetView.ToRegionalBalanceTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalBalanceTargetView.ToRegionalBalanceTargetViews(new List<RegionalBalanceTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalBalanceTargetViewShowsTheStatusInCapitals();
    }
}
