namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalAttestationCheckpointChecks
{
    public static void GlobalAttestationCheckpointStartsInIdle()
    {
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("code-291", "label-713");
        Check.Equal(item.Status, GlobalAttestationCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAttestationCheckpointMovesToWarming()
    {
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("code-291", "label-713");
        try
        {
            item.MoveTo(GlobalAttestationCheckpointStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAttestationCheckpointStatus.Warming, "status");
    }

    public static void GlobalAttestationCheckpointRejectsMovesFromStopped()
    {
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("code-291", "label-713");
        item.Status = GlobalAttestationCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalAttestationCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalAttestationCheckpointStartsInIdle();
        GlobalAttestationCheckpointMovesToWarming();
        GlobalAttestationCheckpointRejectsMovesFromStopped();
    }
}
