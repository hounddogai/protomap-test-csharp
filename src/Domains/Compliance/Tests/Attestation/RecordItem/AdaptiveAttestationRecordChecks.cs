namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveAttestationRecordChecks
{
    public static void AdaptiveAttestationRecordStartsInDraft()
    {
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("code-640", "label-295");
        Check.Equal(item.Status, AdaptiveAttestationRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAttestationRecordMovesToActive()
    {
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("code-640", "label-295");
        try
        {
            item.MoveTo(AdaptiveAttestationRecordStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAttestationRecordStatus.Active, "status");
    }

    public static void AdaptiveAttestationRecordRejectsMovesFromClosed()
    {
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("code-640", "label-295");
        item.Status = AdaptiveAttestationRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveAttestationRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveAttestationRecordStartsInDraft();
        AdaptiveAttestationRecordMovesToActive();
        AdaptiveAttestationRecordRejectsMovesFromClosed();
    }
}
