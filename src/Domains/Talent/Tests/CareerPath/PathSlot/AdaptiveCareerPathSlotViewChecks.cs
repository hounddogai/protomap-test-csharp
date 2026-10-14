namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class AdaptiveCareerPathSlotViewChecks
{
    public static void AdaptiveCareerPathSlotViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("code-31", "title-708");
        var view = AdaptiveCareerPathSlotView.ToAdaptiveCareerPathSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveCareerPathSlotView.ToAdaptiveCareerPathSlotViews(new List<AdaptiveCareerPathSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathSlotViewShowsTheStatusInCapitals();
    }
}
