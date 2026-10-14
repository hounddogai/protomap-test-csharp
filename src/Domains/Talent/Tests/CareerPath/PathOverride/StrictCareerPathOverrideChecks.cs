namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class StrictCareerPathOverrideChecks
{
    public static void StrictCareerPathOverrideStartsInProposed()
    {
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("code-51", "title-114");
        Check.Equal(item.Status, StrictCareerPathOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictCareerPathOverrideMovesToApproved()
    {
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("code-51", "title-114");
        try
        {
            item.MoveTo(StrictCareerPathOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictCareerPathOverrideStatus.Approved, "status");
    }

    public static void StrictCareerPathOverrideRejectsMovesFromWithdrawn()
    {
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("code-51", "title-114");
        item.Status = StrictCareerPathOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictCareerPathOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictCareerPathOverrideStartsInProposed();
        StrictCareerPathOverrideMovesToApproved();
        StrictCareerPathOverrideRejectsMovesFromWithdrawn();
    }
}
