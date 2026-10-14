namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationScoreChecks
{
    public static void PrimaryAttestationScoreStartsInOpen()
    {
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("code-217", "name-892");
        Check.Equal(item.Status, PrimaryAttestationScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAttestationScoreMovesToInReview()
    {
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("code-217", "name-892");
        try
        {
            item.MoveTo(PrimaryAttestationScoreStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAttestationScoreStatus.InReview, "status");
    }

    public static void PrimaryAttestationScoreRejectsMovesFromArchived()
    {
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("code-217", "name-892");
        item.Status = PrimaryAttestationScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryAttestationScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryAttestationScoreStartsInOpen();
        PrimaryAttestationScoreMovesToInReview();
        PrimaryAttestationScoreRejectsMovesFromArchived();
    }
}
