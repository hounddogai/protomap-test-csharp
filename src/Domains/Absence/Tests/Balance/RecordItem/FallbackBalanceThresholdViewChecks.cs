namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FallbackBalanceThresholdViewChecks
{
    public static void FallbackBalanceThresholdViewShowsTheStatusInCapitals()
    {
        var item = FallbackBalanceThreshold.NewFallbackBalanceThreshold("code-680", "title-535");
        var view = FallbackBalanceThresholdView.ToFallbackBalanceThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackBalanceThresholdView.ToFallbackBalanceThresholdViews(new List<FallbackBalanceThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackBalanceThresholdViewShowsTheStatusInCapitals();
    }
}
