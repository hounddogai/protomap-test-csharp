namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TieredScenarioDraftViewChecks
{
    public static void TieredScenarioDraftViewShowsTheStatusInCapitals()
    {
        var item = TieredScenarioDraft.NewTieredScenarioDraft("code-290", "label-834");
        var view = TieredScenarioDraftView.ToTieredScenarioDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredScenarioDraftView.ToTieredScenarioDraftViews(new List<TieredScenarioDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredScenarioDraftViewShowsTheStatusInCapitals();
    }
}
