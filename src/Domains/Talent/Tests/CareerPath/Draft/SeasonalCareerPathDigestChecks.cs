namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SeasonalCareerPathDigestChecks
{
    public static void SeasonalCareerPathDigestStartsInRequested()
    {
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("code-972", "label-577");
        Check.Equal(item.Status, SeasonalCareerPathDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalCareerPathDigestMovesToExpired()
    {
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("code-972", "label-577");
        try
        {
            item.MoveTo(SeasonalCareerPathDigestStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalCareerPathDigestStatus.Expired, "status");
    }

    public static void SeasonalCareerPathDigestRejectsMovesFromExpired()
    {
        var item = SeasonalCareerPathDigest.NewSeasonalCareerPathDigest("code-972", "label-577");
        item.Status = SeasonalCareerPathDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalCareerPathDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalCareerPathDigestStartsInRequested();
        SeasonalCareerPathDigestMovesToExpired();
        SeasonalCareerPathDigestRejectsMovesFromExpired();
    }
}
