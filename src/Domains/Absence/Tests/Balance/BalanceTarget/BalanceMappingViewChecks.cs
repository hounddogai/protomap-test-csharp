namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceMappingViewChecks
{
    public static void BalanceMappingViewShowsTheStatusInCapitals()
    {
        var item = BalanceMapping.NewBalanceMapping("code-102", "title-283");
        var view = BalanceMappingView.ToBalanceMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceMappingView.ToBalanceMappingViews(new List<BalanceMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceMappingViewShowsTheStatusInCapitals();
    }
}
