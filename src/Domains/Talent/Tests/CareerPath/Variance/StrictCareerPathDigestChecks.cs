namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictCareerPathDigestChecks
{
    public static void StrictCareerPathDigestStartsInProposed()
    {
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("code-126", "name-233");
        Check.Equal(item.Status, StrictCareerPathDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictCareerPathDigestMovesToRejected()
    {
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("code-126", "name-233");
        try
        {
            item.MoveTo(StrictCareerPathDigestStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictCareerPathDigestStatus.Rejected, "status");
    }

    public static void StrictCareerPathDigestRejectsMovesFromWithdrawn()
    {
        var item = StrictCareerPathDigest.NewStrictCareerPathDigest("code-126", "name-233");
        item.Status = StrictCareerPathDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictCareerPathDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictCareerPathDigestStartsInProposed();
        StrictCareerPathDigestMovesToRejected();
        StrictCareerPathDigestRejectsMovesFromWithdrawn();
    }
}
