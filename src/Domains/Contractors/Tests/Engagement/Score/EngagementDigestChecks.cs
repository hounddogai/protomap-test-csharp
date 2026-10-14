namespace Acme.Workforce.Contractors.Tests.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class EngagementDigestChecks
{
    public static void EngagementDigestStartsInPlanned()
    {
        var item = EngagementDigest.NewEngagementDigest("code-639", "name-312");
        Check.Equal(item.Status, EngagementDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementDigestMovesToInProgress()
    {
        var item = EngagementDigest.NewEngagementDigest("code-639", "name-312");
        try
        {
            item.MoveTo(EngagementDigestStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementDigestStatus.InProgress, "status");
    }

    public static void EngagementDigestRejectsMovesFromCancelled()
    {
        var item = EngagementDigest.NewEngagementDigest("code-639", "name-312");
        item.Status = EngagementDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(EngagementDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        EngagementDigestStartsInPlanned();
        EngagementDigestMovesToInProgress();
        EngagementDigestRejectsMovesFromCancelled();
    }
}
