namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationEntryChecks
{
    public static void AttestationEntryStartsInProposed()
    {
        var item = AttestationEntry.NewAttestationEntry("code-882", "label-308");
        Check.Equal(item.Status, AttestationEntryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationEntryMovesToRejected()
    {
        var item = AttestationEntry.NewAttestationEntry("code-882", "label-308");
        try
        {
            item.MoveTo(AttestationEntryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationEntryStatus.Rejected, "status");
    }

    public static void AttestationEntryRejectsMovesFromWithdrawn()
    {
        var item = AttestationEntry.NewAttestationEntry("code-882", "label-308");
        item.Status = AttestationEntryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AttestationEntryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AttestationEntryStartsInProposed();
        AttestationEntryMovesToRejected();
        AttestationEntryRejectsMovesFromWithdrawn();
    }
}
