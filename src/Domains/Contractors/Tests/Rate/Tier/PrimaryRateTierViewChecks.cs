namespace Acme.Workforce.Contractors.Tests.Rate.Tier;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PrimaryRateTierViewChecks
{
    public static void PrimaryRateTierViewShowsTheStatusInCapitals()
    {
        var item = PrimaryRateTier.NewPrimaryRateTier("code-353", "title-235");
        var view = PrimaryRateTierView.ToPrimaryRateTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryRateTierView.ToPrimaryRateTierViews(new List<PrimaryRateTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryRateTierViewShowsTheStatusInCapitals();
    }
}
