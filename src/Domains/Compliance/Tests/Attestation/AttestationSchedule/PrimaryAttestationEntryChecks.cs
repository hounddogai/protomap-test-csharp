namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationEntryChecks
{
    public static void PrimaryAttestationEntryStartsInPlanned()
    {
        var item = PrimaryAttestationEntry.NewPrimaryAttestationEntry("code-243", "label-971");
        Check.Equal(item.Status, PrimaryAttestationEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAttestationEntryMovesToInProgress()
    {
        var item = PrimaryAttestationEntry.NewPrimaryAttestationEntry("code-243", "label-971");
        try
        {
            item.MoveTo(PrimaryAttestationEntryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAttestationEntryStatus.InProgress, "status");
    }

    public static void PrimaryAttestationEntryRejectsMovesFromCancelled()
    {
        var item = PrimaryAttestationEntry.NewPrimaryAttestationEntry("code-243", "label-971");
        item.Status = PrimaryAttestationEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryAttestationEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryAttestationEntryStartsInPlanned();
        PrimaryAttestationEntryMovesToInProgress();
        PrimaryAttestationEntryRejectsMovesFromCancelled();
    }
}
