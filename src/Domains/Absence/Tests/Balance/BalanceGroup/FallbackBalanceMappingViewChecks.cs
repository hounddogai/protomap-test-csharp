namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class FallbackBalanceMappingViewChecks
{
    public static void FallbackBalanceMappingViewShowsTheStatusInCapitals()
    {
        var item = FallbackBalanceMapping.NewFallbackBalanceMapping("code-150", "title-243");
        var view = FallbackBalanceMappingView.ToFallbackBalanceMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackBalanceMappingView.ToFallbackBalanceMappingViews(new List<FallbackBalanceMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackBalanceMappingViewShowsTheStatusInCapitals();
    }
}
