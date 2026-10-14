namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class AttestationAdjustmentChecks
{
    public static void AttestationAdjustmentStartsInDraft()
    {
        var item = AttestationAdjustment.NewAttestationAdjustment("code-15", "title-194");
        Check.Equal(item.Status, AttestationAdjustmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationAdjustmentMovesToClosed()
    {
        var item = AttestationAdjustment.NewAttestationAdjustment("code-15", "title-194");
        try
        {
            item.MoveTo(AttestationAdjustmentStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationAdjustmentStatus.Closed, "status");
    }

    public static void AttestationAdjustmentRejectsMovesFromClosed()
    {
        var item = AttestationAdjustment.NewAttestationAdjustment("code-15", "title-194");
        item.Status = AttestationAdjustmentStatus.Closed;
        Check.Fails(() => item.MoveTo(AttestationAdjustmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AttestationAdjustmentStartsInDraft();
        AttestationAdjustmentMovesToClosed();
        AttestationAdjustmentRejectsMovesFromClosed();
    }
}
