namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LegacyCertificationDigestChecks
{
    public static void LegacyCertificationDigestStartsInOpen()
    {
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("code-70", "name-566");
        Check.Equal(item.Status, LegacyCertificationDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyCertificationDigestMovesToResolved()
    {
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("code-70", "name-566");
        try
        {
            item.MoveTo(LegacyCertificationDigestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyCertificationDigestStatus.Resolved, "status");
    }

    public static void LegacyCertificationDigestRejectsMovesFromArchived()
    {
        var item = LegacyCertificationDigest.NewLegacyCertificationDigest("code-70", "name-566");
        item.Status = LegacyCertificationDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacyCertificationDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacyCertificationDigestStartsInOpen();
        LegacyCertificationDigestMovesToResolved();
        LegacyCertificationDigestRejectsMovesFromArchived();
    }
}
