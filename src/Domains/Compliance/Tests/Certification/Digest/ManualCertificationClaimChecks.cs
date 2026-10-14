namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ManualCertificationClaimChecks
{
    public static void ManualCertificationClaimStartsInPlanned()
    {
        var item = ManualCertificationClaim.NewManualCertificationClaim("code-283", "name-54");
        Check.Equal(item.Status, ManualCertificationClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualCertificationClaimMovesToInProgress()
    {
        var item = ManualCertificationClaim.NewManualCertificationClaim("code-283", "name-54");
        try
        {
            item.MoveTo(ManualCertificationClaimStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualCertificationClaimStatus.InProgress, "status");
    }

    public static void ManualCertificationClaimRejectsMovesFromCancelled()
    {
        var item = ManualCertificationClaim.NewManualCertificationClaim("code-283", "name-54");
        item.Status = ManualCertificationClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualCertificationClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualCertificationClaimStartsInPlanned();
        ManualCertificationClaimMovesToInProgress();
        ManualCertificationClaimRejectsMovesFromCancelled();
    }
}
