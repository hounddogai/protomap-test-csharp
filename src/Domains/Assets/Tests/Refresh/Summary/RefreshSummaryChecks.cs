namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RefreshSummaryChecks
{
    public static void RefreshSummaryStartsInPlanned()
    {
        var item = RefreshSummary.NewRefreshSummary("code-489", "label-615");
        Check.Equal(item.Status, RefreshSummaryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshSummaryMovesToInProgress()
    {
        var item = RefreshSummary.NewRefreshSummary("code-489", "label-615");
        try
        {
            item.MoveTo(RefreshSummaryStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshSummaryStatus.InProgress, "status");
    }

    public static void RefreshSummaryRejectsMovesFromCancelled()
    {
        var item = RefreshSummary.NewRefreshSummary("code-489", "label-615");
        item.Status = RefreshSummaryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RefreshSummaryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RefreshSummaryStartsInPlanned();
        RefreshSummaryMovesToInProgress();
        RefreshSummaryRejectsMovesFromCancelled();
    }
}
