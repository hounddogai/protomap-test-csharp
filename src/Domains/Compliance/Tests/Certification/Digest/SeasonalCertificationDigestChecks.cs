namespace Acme.Workforce.Compliance.Tests.Certification.Digest;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SeasonalCertificationDigestChecks
{
    public static void SeasonalCertificationDigestStartsInProposed()
    {
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("code-15", "label-264");
        Check.Equal(item.Status, SeasonalCertificationDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalCertificationDigestMovesToApproved()
    {
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("code-15", "label-264");
        try
        {
            item.MoveTo(SeasonalCertificationDigestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalCertificationDigestStatus.Approved, "status");
    }

    public static void SeasonalCertificationDigestRejectsMovesFromWithdrawn()
    {
        var item = SeasonalCertificationDigest.NewSeasonalCertificationDigest("code-15", "label-264");
        item.Status = SeasonalCertificationDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalCertificationDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalCertificationDigestStartsInProposed();
        SeasonalCertificationDigestMovesToApproved();
        SeasonalCertificationDigestRejectsMovesFromWithdrawn();
    }
}
