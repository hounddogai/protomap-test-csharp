namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ManualRateOverrideViewChecks
{
    public static void ManualRateOverrideViewShowsTheStatusInCapitals()
    {
        var item = ManualRateOverride.NewManualRateOverride("code-391", "name-173");
        var view = ManualRateOverrideView.ToManualRateOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualRateOverrideView.ToManualRateOverrideViews(new List<ManualRateOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualRateOverrideViewShowsTheStatusInCapitals();
    }
}
