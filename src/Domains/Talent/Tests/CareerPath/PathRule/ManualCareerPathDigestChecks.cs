namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class ManualCareerPathDigestChecks
{
    public static void ManualCareerPathDigestStartsInRequested()
    {
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("code-966", "label-8");
        Check.Equal(item.Status, ManualCareerPathDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualCareerPathDigestMovesToGranted()
    {
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("code-966", "label-8");
        try
        {
            item.MoveTo(ManualCareerPathDigestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualCareerPathDigestStatus.Granted, "status");
    }

    public static void ManualCareerPathDigestRejectsMovesFromExpired()
    {
        var item = ManualCareerPathDigest.NewManualCareerPathDigest("code-966", "label-8");
        item.Status = ManualCareerPathDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualCareerPathDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualCareerPathDigestStartsInRequested();
        ManualCareerPathDigestMovesToGranted();
        ManualCareerPathDigestRejectsMovesFromExpired();
    }
}
