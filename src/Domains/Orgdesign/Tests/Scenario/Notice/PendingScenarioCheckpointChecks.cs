namespace Acme.Workforce.Orgdesign.Tests.Scenario.Notice;

using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingScenarioCheckpointChecks
{
    public static void PendingScenarioCheckpointStartsInProposed()
    {
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("code-286", "label-752");
        Check.Equal(item.Status, PendingScenarioCheckpointStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingScenarioCheckpointMovesToWithdrawn()
    {
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("code-286", "label-752");
        try
        {
            item.MoveTo(PendingScenarioCheckpointStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingScenarioCheckpointStatus.Withdrawn, "status");
    }

    public static void PendingScenarioCheckpointRejectsMovesFromWithdrawn()
    {
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("code-286", "label-752");
        item.Status = PendingScenarioCheckpointStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingScenarioCheckpointStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingScenarioCheckpointStartsInProposed();
        PendingScenarioCheckpointMovesToWithdrawn();
        PendingScenarioCheckpointRejectsMovesFromWithdrawn();
    }
}
