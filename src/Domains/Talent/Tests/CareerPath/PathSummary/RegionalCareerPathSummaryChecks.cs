namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class RegionalCareerPathSummaryChecks
{
    public static void RegionalCareerPathSummaryStartsInIdle()
    {
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("code-498", "title-732");
        Check.Equal(item.Status, RegionalCareerPathSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalCareerPathSummaryMovesToStopped()
    {
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("code-498", "title-732");
        try
        {
            item.MoveTo(RegionalCareerPathSummaryStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalCareerPathSummaryStatus.Stopped, "status");
    }

    public static void RegionalCareerPathSummaryRejectsMovesFromStopped()
    {
        var item = RegionalCareerPathSummary.NewRegionalCareerPathSummary("code-498", "title-732");
        item.Status = RegionalCareerPathSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalCareerPathSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalCareerPathSummaryStartsInIdle();
        RegionalCareerPathSummaryMovesToStopped();
        RegionalCareerPathSummaryRejectsMovesFromStopped();
    }
}
