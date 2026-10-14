namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyAttestationEntryChecks
{
    public static void LegacyAttestationEntryStartsInPlanned()
    {
        var item = LegacyAttestationEntry.NewLegacyAttestationEntry("code-377", "label-36");
        Check.Equal(item.Status, LegacyAttestationEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyAttestationEntryMovesToInProgress()
    {
        var item = LegacyAttestationEntry.NewLegacyAttestationEntry("code-377", "label-36");
        try
        {
            item.MoveTo(LegacyAttestationEntryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyAttestationEntryStatus.InProgress, "status");
    }

    public static void LegacyAttestationEntryRejectsMovesFromCancelled()
    {
        var item = LegacyAttestationEntry.NewLegacyAttestationEntry("code-377", "label-36");
        item.Status = LegacyAttestationEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyAttestationEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyAttestationEntryStartsInPlanned();
        LegacyAttestationEntryMovesToInProgress();
        LegacyAttestationEntryRejectsMovesFromCancelled();
    }
}
