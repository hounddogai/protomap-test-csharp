namespace Acme.Workforce.Compliance.Tests.Attestation.Batch;

using Acme.Workforce.Compliance.Attestation.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AttestationBatchChecks
{
    public static void AttestationBatchStartsInNew()
    {
        var item = AttestationBatch.NewAttestationBatch("code-454", "name-300");
        Check.Equal(item.Status, AttestationBatchStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationBatchMovesToDone()
    {
        var item = AttestationBatch.NewAttestationBatch("code-454", "name-300");
        try
        {
            item.MoveTo(AttestationBatchStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationBatchStatus.Done, "status");
    }

    public static void AttestationBatchRejectsMovesFromFailed()
    {
        var item = AttestationBatch.NewAttestationBatch("code-454", "name-300");
        item.Status = AttestationBatchStatus.Failed;
        Check.Fails(() => item.MoveTo(AttestationBatchStatus.New), "move");
    }

    public static void RunAll()
    {
        AttestationBatchStartsInNew();
        AttestationBatchMovesToDone();
        AttestationBatchRejectsMovesFromFailed();
    }
}
