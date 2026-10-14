namespace Acme.Workforce.Compliance.Tests.Attestation.Rollup;

using Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AttestationRecordChecks
{
    public static void AttestationRecordStartsInOpen()
    {
        var item = AttestationRecord.NewAttestationRecord("code-428", "name-765");
        Check.Equal(item.Status, AttestationRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationRecordMovesToArchived()
    {
        var item = AttestationRecord.NewAttestationRecord("code-428", "name-765");
        try
        {
            item.MoveTo(AttestationRecordStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationRecordStatus.Archived, "status");
    }

    public static void AttestationRecordRejectsMovesFromArchived()
    {
        var item = AttestationRecord.NewAttestationRecord("code-428", "name-765");
        item.Status = AttestationRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(AttestationRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        AttestationRecordStartsInOpen();
        AttestationRecordMovesToArchived();
        AttestationRecordRejectsMovesFromArchived();
    }
}
