namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayTierViewChecks
{
    public static void RetroPayTierViewShowsTheStatusInCapitals()
    {
        var item = RetroPayTier.NewRetroPayTier("code-252", "label-61");
        var view = RetroPayTierView.ToRetroPayTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayTierView.ToRetroPayTierViews(new List<RetroPayTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayTierViewShowsTheStatusInCapitals();
    }
}
