namespace Acme.Workforce.Compliance.Tests.Certification.Slot;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SeasonalCertificationManifestChecks
{
    public static void SeasonalCertificationManifestStartsInOpen()
    {
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("code-271", "label-448");
        Check.Equal(item.Status, SeasonalCertificationManifestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalCertificationManifestMovesToResolved()
    {
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("code-271", "label-448");
        try
        {
            item.MoveTo(SeasonalCertificationManifestStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalCertificationManifestStatus.Resolved, "status");
    }

    public static void SeasonalCertificationManifestRejectsMovesFromArchived()
    {
        var item = SeasonalCertificationManifest.NewSeasonalCertificationManifest("code-271", "label-448");
        item.Status = SeasonalCertificationManifestStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalCertificationManifestStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalCertificationManifestStartsInOpen();
        SeasonalCertificationManifestMovesToResolved();
        SeasonalCertificationManifestRejectsMovesFromArchived();
    }
}
