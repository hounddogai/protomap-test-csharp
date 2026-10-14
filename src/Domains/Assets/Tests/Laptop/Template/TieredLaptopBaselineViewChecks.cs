namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredLaptopBaselineViewChecks
{
    public static void TieredLaptopBaselineViewShowsTheStatusInCapitals()
    {
        var item = TieredLaptopBaseline.NewTieredLaptopBaseline("code-902", "title-475");
        var view = TieredLaptopBaselineView.ToTieredLaptopBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredLaptopBaselineView.ToTieredLaptopBaselineViews(new List<TieredLaptopBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredLaptopBaselineViewShowsTheStatusInCapitals();
    }
}
