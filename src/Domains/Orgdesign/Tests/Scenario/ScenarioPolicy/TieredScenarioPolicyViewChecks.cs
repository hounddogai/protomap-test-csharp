namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class TieredScenarioPolicyViewChecks
{
    public static void TieredScenarioPolicyViewShowsTheStatusInCapitals()
    {
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("code-485", "title-297");
        var view = TieredScenarioPolicyView.ToTieredScenarioPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredScenarioPolicyView.ToTieredScenarioPolicyViews(new List<TieredScenarioPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredScenarioPolicyViewShowsTheStatusInCapitals();
    }
}
