namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacyCareerPathDigestChecks
{
    public static void LegacyCareerPathDigestStartsInNew()
    {
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("code-524", "title-28");
        Check.Equal(item.Status, LegacyCareerPathDigestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyCareerPathDigestMovesToQueued()
    {
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("code-524", "title-28");
        try
        {
            item.MoveTo(LegacyCareerPathDigestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyCareerPathDigestStatus.Queued, "status");
    }

    public static void LegacyCareerPathDigestRejectsMovesFromFailed()
    {
        var item = LegacyCareerPathDigest.NewLegacyCareerPathDigest("code-524", "title-28");
        item.Status = LegacyCareerPathDigestStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyCareerPathDigestStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyCareerPathDigestStartsInNew();
        LegacyCareerPathDigestMovesToQueued();
        LegacyCareerPathDigestRejectsMovesFromFailed();
    }
}
