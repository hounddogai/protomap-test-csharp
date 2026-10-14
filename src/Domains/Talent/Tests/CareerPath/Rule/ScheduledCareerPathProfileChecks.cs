namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScheduledCareerPathProfileChecks
{
    public static void ScheduledCareerPathProfileStartsInIdle()
    {
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("code-38", "title-752");
        Check.Equal(item.Status, ScheduledCareerPathProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledCareerPathProfileMovesToReady()
    {
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("code-38", "title-752");
        try
        {
            item.MoveTo(ScheduledCareerPathProfileStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledCareerPathProfileStatus.Ready, "status");
    }

    public static void ScheduledCareerPathProfileRejectsMovesFromStopped()
    {
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("code-38", "title-752");
        item.Status = ScheduledCareerPathProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledCareerPathProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledCareerPathProfileStartsInIdle();
        ScheduledCareerPathProfileMovesToReady();
        ScheduledCareerPathProfileRejectsMovesFromStopped();
    }
}
