namespace Acme.Workforce.Compliance.Tests.Attestation.Draft;

using Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AttestationDraftChecks
{
    public static void AttestationDraftStartsInNew()
    {
        var item = AttestationDraft.NewAttestationDraft("code-897", "label-47");
        Check.Equal(item.Status, AttestationDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationDraftMovesToFailed()
    {
        var item = AttestationDraft.NewAttestationDraft("code-897", "label-47");
        try
        {
            item.MoveTo(AttestationDraftStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationDraftStatus.Failed, "status");
    }

    public static void AttestationDraftRejectsMovesFromFailed()
    {
        var item = AttestationDraft.NewAttestationDraft("code-897", "label-47");
        item.Status = AttestationDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(AttestationDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        AttestationDraftStartsInNew();
        AttestationDraftMovesToFailed();
        AttestationDraftRejectsMovesFromFailed();
    }
}
