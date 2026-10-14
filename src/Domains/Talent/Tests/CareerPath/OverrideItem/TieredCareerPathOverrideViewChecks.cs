namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredCareerPathOverrideViewChecks
{
    public static void TieredCareerPathOverrideViewShowsTheStatusInCapitals()
    {
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("code-643", "label-595");
        var view = TieredCareerPathOverrideView.ToTieredCareerPathOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredCareerPathOverrideView.ToTieredCareerPathOverrideViews(new List<TieredCareerPathOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredCareerPathOverrideViewShowsTheStatusInCapitals();
    }
}
