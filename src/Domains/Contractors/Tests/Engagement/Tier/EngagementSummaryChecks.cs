namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementSummaryChecks
{
    public static void EngagementSummaryStartsInNew()
    {
        var item = EngagementSummary.NewEngagementSummary("code-595", "label-187");
        Check.Equal(item.Status, EngagementSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementSummaryMovesToQueued()
    {
        var item = EngagementSummary.NewEngagementSummary("code-595", "label-187");
        try
        {
            item.MoveTo(EngagementSummaryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementSummaryStatus.Queued, "status");
    }

    public static void EngagementSummaryRejectsMovesFromFailed()
    {
        var item = EngagementSummary.NewEngagementSummary("code-595", "label-187");
        item.Status = EngagementSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(EngagementSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        EngagementSummaryStartsInNew();
        EngagementSummaryMovesToQueued();
        EngagementSummaryRejectsMovesFromFailed();
    }
}
