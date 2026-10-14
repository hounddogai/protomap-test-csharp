namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class PrimaryScenarioSegmentChecks
{
    public static void PrimaryScenarioSegmentStartsInDraft()
    {
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("code-659", "title-698");
        Check.Equal(item.Status, PrimaryScenarioSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryScenarioSegmentMovesToActive()
    {
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("code-659", "title-698");
        try
        {
            item.MoveTo(PrimaryScenarioSegmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryScenarioSegmentStatus.Active, "status");
    }

    public static void PrimaryScenarioSegmentRejectsMovesFromClosed()
    {
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("code-659", "title-698");
        item.Status = PrimaryScenarioSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryScenarioSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryScenarioSegmentStartsInDraft();
        PrimaryScenarioSegmentMovesToActive();
        PrimaryScenarioSegmentRejectsMovesFromClosed();
    }
}
