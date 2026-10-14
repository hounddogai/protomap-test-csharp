namespace Acme.Workforce.Absence.Tests.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BalanceBundleViewChecks
{
    public static void BalanceBundleViewShowsTheStatusInCapitals()
    {
        var item = BalanceBundle.NewBalanceBundle("code-167", "label-723");
        var view = BalanceBundleView.ToBalanceBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceBundleView.ToBalanceBundleViews(new List<BalanceBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceBundleViewShowsTheStatusInCapitals();
    }
}
