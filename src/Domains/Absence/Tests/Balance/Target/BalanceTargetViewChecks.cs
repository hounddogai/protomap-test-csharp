namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceTargetViewChecks
{
    public static void BalanceTargetViewShowsTheStatusInCapitals()
    {
        var item = BalanceTarget.NewBalanceTarget("code-89", "name-233");
        var view = BalanceTargetView.ToBalanceTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceTargetView.ToBalanceTargetViews(new List<BalanceTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceTargetViewShowsTheStatusInCapitals();
    }
}
