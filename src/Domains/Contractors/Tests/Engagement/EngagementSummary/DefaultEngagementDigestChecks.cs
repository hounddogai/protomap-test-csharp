namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class DefaultEngagementDigestChecks
{
    public static void DefaultEngagementDigestStartsInDraft()
    {
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("code-22", "label-202");
        Check.Equal(item.Status, DefaultEngagementDigestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultEngagementDigestMovesToClosed()
    {
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("code-22", "label-202");
        try
        {
            item.MoveTo(DefaultEngagementDigestStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultEngagementDigestStatus.Closed, "status");
    }

    public static void DefaultEngagementDigestRejectsMovesFromClosed()
    {
        var item = DefaultEngagementDigest.NewDefaultEngagementDigest("code-22", "label-202");
        item.Status = DefaultEngagementDigestStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultEngagementDigestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultEngagementDigestStartsInDraft();
        DefaultEngagementDigestMovesToClosed();
        DefaultEngagementDigestRejectsMovesFromClosed();
    }
}
