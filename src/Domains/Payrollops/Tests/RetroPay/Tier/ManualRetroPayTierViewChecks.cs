namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualRetroPayTierViewChecks
{
    public static void ManualRetroPayTierViewShowsTheStatusInCapitals()
    {
        var item = ManualRetroPayTier.NewManualRetroPayTier("code-434", "name-959");
        var view = ManualRetroPayTierView.ToManualRetroPayTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualRetroPayTierView.ToManualRetroPayTierViews(new List<ManualRetroPayTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualRetroPayTierViewShowsTheStatusInCapitals();
    }
}
