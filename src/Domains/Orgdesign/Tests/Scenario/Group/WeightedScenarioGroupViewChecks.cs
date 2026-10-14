namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedScenarioGroupViewChecks
{
    public static void WeightedScenarioGroupViewShowsTheStatusInCapitals()
    {
        var item = WeightedScenarioGroup.NewWeightedScenarioGroup("code-267", "title-172");
        var view = WeightedScenarioGroupView.ToWeightedScenarioGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedScenarioGroupView.ToWeightedScenarioGroupViews(new List<WeightedScenarioGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedScenarioGroupViewShowsTheStatusInCapitals();
    }
}
