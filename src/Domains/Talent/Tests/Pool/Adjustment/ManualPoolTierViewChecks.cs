namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualPoolTierViewChecks
{
    public static void ManualPoolTierViewShowsTheStatusInCapitals()
    {
        var item = ManualPoolTier.NewManualPoolTier("code-855", "name-647");
        var view = ManualPoolTierView.ToManualPoolTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPoolTierView.ToManualPoolTierViews(new List<ManualPoolTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPoolTierViewShowsTheStatusInCapitals();
    }
}
