namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class AdaptiveAttestationDraftChecks
{
    public static void AdaptiveAttestationDraftStartsInDraft()
    {
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("code-277", "name-660");
        Check.Equal(item.Status, AdaptiveAttestationDraftStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAttestationDraftMovesToPending()
    {
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("code-277", "name-660");
        try
        {
            item.MoveTo(AdaptiveAttestationDraftStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAttestationDraftStatus.Pending, "status");
    }

    public static void AdaptiveAttestationDraftRejectsMovesFromClosed()
    {
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("code-277", "name-660");
        item.Status = AdaptiveAttestationDraftStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveAttestationDraftStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveAttestationDraftStartsInDraft();
        AdaptiveAttestationDraftMovesToPending();
        AdaptiveAttestationDraftRejectsMovesFromClosed();
    }
}
