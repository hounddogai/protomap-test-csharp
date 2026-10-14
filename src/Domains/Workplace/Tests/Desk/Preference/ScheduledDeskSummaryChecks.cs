namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledDeskSummaryChecks
{
    public static void ScheduledDeskSummaryStartsInDraft()
    {
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("code-698", "title-469");
        Check.Equal(item.Status, ScheduledDeskSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledDeskSummaryMovesToPending()
    {
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("code-698", "title-469");
        try
        {
            item.MoveTo(ScheduledDeskSummaryStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledDeskSummaryStatus.Pending, "status");
    }

    public static void ScheduledDeskSummaryRejectsMovesFromClosed()
    {
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("code-698", "title-469");
        item.Status = ScheduledDeskSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledDeskSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledDeskSummaryStartsInDraft();
        ScheduledDeskSummaryMovesToPending();
        ScheduledDeskSummaryRejectsMovesFromClosed();
    }
}
