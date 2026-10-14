namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class GlobalScenarioBundleViewChecks
{
    public static void GlobalScenarioBundleViewShowsTheStatusInCapitals()
    {
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("code-872", "label-866");
        var view = GlobalScenarioBundleView.ToGlobalScenarioBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalScenarioBundleView.ToGlobalScenarioBundleViews(new List<GlobalScenarioBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalScenarioBundleViewShowsTheStatusInCapitals();
    }
}
