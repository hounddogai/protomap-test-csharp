namespace Acme.Workforce.Talent.Tests.CareerPath.Profile;

using Acme.Workforce.Talent.CareerPath.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingCareerPathProfileChecks
{
    public static void RollingCareerPathProfileStartsInOpen()
    {
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("code-638", "name-465");
        Check.Equal(item.Status, RollingCareerPathProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingCareerPathProfileMovesToReopened()
    {
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("code-638", "name-465");
        try
        {
            item.MoveTo(RollingCareerPathProfileStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingCareerPathProfileStatus.Reopened, "status");
    }

    public static void RollingCareerPathProfileRejectsMovesFromArchived()
    {
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("code-638", "name-465");
        item.Status = RollingCareerPathProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingCareerPathProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingCareerPathProfileStartsInOpen();
        RollingCareerPathProfileMovesToReopened();
        RollingCareerPathProfileRejectsMovesFromArchived();
    }
}
