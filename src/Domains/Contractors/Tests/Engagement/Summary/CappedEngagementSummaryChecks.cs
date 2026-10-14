namespace Acme.Workforce.Contractors.Tests.Engagement.Summary;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CappedEngagementSummaryChecks
{
    public static void CappedEngagementSummaryStartsInRequested()
    {
        var item = CappedEngagementSummary.NewCappedEngagementSummary("code-89", "label-840");
        Check.Equal(item.Status, CappedEngagementSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedEngagementSummaryMovesToRevoked()
    {
        var item = CappedEngagementSummary.NewCappedEngagementSummary("code-89", "label-840");
        try
        {
            item.MoveTo(CappedEngagementSummaryStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedEngagementSummaryStatus.Revoked, "status");
    }

    public static void CappedEngagementSummaryRejectsMovesFromExpired()
    {
        var item = CappedEngagementSummary.NewCappedEngagementSummary("code-89", "label-840");
        item.Status = CappedEngagementSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedEngagementSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedEngagementSummaryStartsInRequested();
        CappedEngagementSummaryMovesToRevoked();
        CappedEngagementSummaryRejectsMovesFromExpired();
    }
}
