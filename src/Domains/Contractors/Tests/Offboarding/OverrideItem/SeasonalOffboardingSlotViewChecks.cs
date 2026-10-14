namespace Acme.Workforce.Contractors.Tests.Offboarding.OverrideItem;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class SeasonalOffboardingSlotViewChecks
{
    public static void SeasonalOffboardingSlotViewShowsTheStatusInCapitals()
    {
        var item = SeasonalOffboardingSlot.NewSeasonalOffboardingSlot("code-506", "name-254");
        var view = SeasonalOffboardingSlotView.ToSeasonalOffboardingSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalOffboardingSlotView.ToSeasonalOffboardingSlotViews(new List<SeasonalOffboardingSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalOffboardingSlotViewShowsTheStatusInCapitals();
    }
}
