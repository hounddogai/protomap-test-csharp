namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScenarioSlotViewChecks
{
    public static void ScenarioSlotViewShowsTheStatusInCapitals()
    {
        var item = ScenarioSlot.NewScenarioSlot("code-853", "name-763");
        var view = ScenarioSlotView.ToScenarioSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioSlotView.ToScenarioSlotViews(new List<ScenarioSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioSlotViewShowsTheStatusInCapitals();
    }
}
