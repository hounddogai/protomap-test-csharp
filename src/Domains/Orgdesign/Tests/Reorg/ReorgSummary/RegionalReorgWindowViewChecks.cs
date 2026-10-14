namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class RegionalReorgWindowViewChecks
{
    public static void RegionalReorgWindowViewShowsTheStatusInCapitals()
    {
        var item = RegionalReorgWindow.NewRegionalReorgWindow("code-644", "label-979");
        var view = RegionalReorgWindowView.ToRegionalReorgWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalReorgWindowView.ToRegionalReorgWindowViews(new List<RegionalReorgWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalReorgWindowViewShowsTheStatusInCapitals();
    }
}
