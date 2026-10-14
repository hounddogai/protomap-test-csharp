namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioSegmentViewChecks
{
    public static void ScenarioSegmentViewShowsTheStatusInCapitals()
    {
        var item = ScenarioSegment.NewScenarioSegment("code-632", "name-976");
        var view = ScenarioSegmentView.ToScenarioSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScenarioSegmentView.ToScenarioSegmentViews(new List<ScenarioSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScenarioSegmentViewShowsTheStatusInCapitals();
    }
}
