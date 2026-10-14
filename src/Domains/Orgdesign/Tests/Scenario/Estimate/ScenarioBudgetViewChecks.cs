namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScenarioBudgetViewChecks
{
    public static void ScenarioBudgetViewShowsTheStatusInCapitals()
    {
        var item = ScenarioBudget.NewScenarioBudget("code-279", "label-139");
        var view = ScenarioBudgetView.ToScenarioBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioBudgetView.ToScenarioBudgetViews(new List<ScenarioBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioBudgetViewShowsTheStatusInCapitals();
    }
}
