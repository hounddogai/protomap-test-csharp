namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class ManualCareerPathProfileChecks
{
    public static void ManualCareerPathProfileStartsInIdle()
    {
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("code-634", "name-428");
        Check.Equal(item.Status, ManualCareerPathProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualCareerPathProfileMovesToDraining()
    {
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("code-634", "name-428");
        try
        {
            item.MoveTo(ManualCareerPathProfileStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualCareerPathProfileStatus.Draining, "status");
    }

    public static void ManualCareerPathProfileRejectsMovesFromStopped()
    {
        var item = ManualCareerPathProfile.NewManualCareerPathProfile("code-634", "name-428");
        item.Status = ManualCareerPathProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualCareerPathProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualCareerPathProfileStartsInIdle();
        ManualCareerPathProfileMovesToDraining();
        ManualCareerPathProfileRejectsMovesFromStopped();
    }
}
