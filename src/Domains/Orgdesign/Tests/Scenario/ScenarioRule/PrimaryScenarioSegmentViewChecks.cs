namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class PrimaryScenarioSegmentViewChecks
{
    public static void PrimaryScenarioSegmentViewShowsTheStatusInCapitals()
    {
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("code-659", "title-698");
        var view = PrimaryScenarioSegmentView.ToPrimaryScenarioSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryScenarioSegmentView.ToPrimaryScenarioSegmentViews(new List<PrimaryScenarioSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryScenarioSegmentViewShowsTheStatusInCapitals();
    }
}
