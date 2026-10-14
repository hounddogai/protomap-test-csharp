namespace Acme.Workforce.Talent.Tests.CareerPath.Summary;

using Acme.Workforce.Talent.CareerPath.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultCareerPathSummaryChecks
{
    public static void DefaultCareerPathSummaryStartsInPlanned()
    {
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("code-990", "title-666");
        Check.Equal(item.Status, DefaultCareerPathSummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultCareerPathSummaryMovesToScheduled()
    {
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("code-990", "title-666");
        try
        {
            item.MoveTo(DefaultCareerPathSummaryStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultCareerPathSummaryStatus.Scheduled, "status");
    }

    public static void DefaultCareerPathSummaryRejectsMovesFromCancelled()
    {
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("code-990", "title-666");
        item.Status = DefaultCareerPathSummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultCareerPathSummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultCareerPathSummaryStartsInPlanned();
        DefaultCareerPathSummaryMovesToScheduled();
        DefaultCareerPathSummaryRejectsMovesFromCancelled();
    }
}
