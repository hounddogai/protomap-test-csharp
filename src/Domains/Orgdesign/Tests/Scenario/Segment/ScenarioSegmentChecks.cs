namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioSegmentChecks
{
    public static void ScenarioSegmentStartsInNew()
    {
        var item = ScenarioSegment.NewScenarioSegment("code-632", "name-976");
        Check.Equal(item.Status, ScenarioSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioSegmentMovesToDone()
    {
        var item = ScenarioSegment.NewScenarioSegment("code-632", "name-976");
        try
        {
            item.MoveTo(ScenarioSegmentStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioSegmentStatus.Done, "status");
    }

    public static void ScenarioSegmentRejectsMovesFromFailed()
    {
        var item = ScenarioSegment.NewScenarioSegment("code-632", "name-976");
        item.Status = ScenarioSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(ScenarioSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        ScenarioSegmentStartsInNew();
        ScenarioSegmentMovesToDone();
        ScenarioSegmentRejectsMovesFromFailed();
    }
}
