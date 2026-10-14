namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BatchedAttestationWindowChecks
{
    public static void BatchedAttestationWindowStartsInIdle()
    {
        var item = BatchedAttestationWindow.NewBatchedAttestationWindow("code-969", "label-833");
        Check.Equal(item.Status, BatchedAttestationWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAttestationWindowMovesToReady()
    {
        var item = BatchedAttestationWindow.NewBatchedAttestationWindow("code-969", "label-833");
        try
        {
            item.MoveTo(BatchedAttestationWindowStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAttestationWindowStatus.Ready, "status");
    }

    public static void BatchedAttestationWindowRejectsMovesFromStopped()
    {
        var item = BatchedAttestationWindow.NewBatchedAttestationWindow("code-969", "label-833");
        item.Status = BatchedAttestationWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedAttestationWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedAttestationWindowStartsInIdle();
        BatchedAttestationWindowMovesToReady();
        BatchedAttestationWindowRejectsMovesFromStopped();
    }
}
