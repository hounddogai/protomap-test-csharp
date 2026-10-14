namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingScenarioRequestChecks
{
    public static void PendingScenarioRequestStartsInPlanned()
    {
        var item = PendingScenarioRequest.NewPendingScenarioRequest("code-596", "label-705");
        Check.Equal(item.Status, PendingScenarioRequestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingScenarioRequestMovesToScheduled()
    {
        var item = PendingScenarioRequest.NewPendingScenarioRequest("code-596", "label-705");
        try
        {
            item.MoveTo(PendingScenarioRequestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingScenarioRequestStatus.Scheduled, "status");
    }

    public static void PendingScenarioRequestRejectsMovesFromCancelled()
    {
        var item = PendingScenarioRequest.NewPendingScenarioRequest("code-596", "label-705");
        item.Status = PendingScenarioRequestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingScenarioRequestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingScenarioRequestStartsInPlanned();
        PendingScenarioRequestMovesToScheduled();
        PendingScenarioRequestRejectsMovesFromCancelled();
    }
}
