namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationScoreChecks
{
    public static void AttestationScoreStartsInDraft()
    {
        var item = AttestationScore.NewAttestationScore("code-144", "title-234");
        Check.Equal(item.Status, AttestationScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationScoreMovesToPending()
    {
        var item = AttestationScore.NewAttestationScore("code-144", "title-234");
        try
        {
            item.MoveTo(AttestationScoreStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationScoreStatus.Pending, "status");
    }

    public static void AttestationScoreRejectsMovesFromClosed()
    {
        var item = AttestationScore.NewAttestationScore("code-144", "title-234");
        item.Status = AttestationScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(AttestationScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AttestationScoreStartsInDraft();
        AttestationScoreMovesToPending();
        AttestationScoreRejectsMovesFromClosed();
    }
}
