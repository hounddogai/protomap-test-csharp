namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredPositionBundleViewChecks
{
    public static void TieredPositionBundleViewShowsTheStatusInCapitals()
    {
        var item = TieredPositionBundle.NewTieredPositionBundle("code-28", "name-162");
        var view = TieredPositionBundleView.ToTieredPositionBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPositionBundleView.ToTieredPositionBundleViews(new List<TieredPositionBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPositionBundleViewShowsTheStatusInCapitals();
    }
}
