namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RollingEngagementDigestChecks
{
    public static void RollingEngagementDigestStartsInNew()
    {
        var item = RollingEngagementDigest.NewRollingEngagementDigest("code-581", "label-491");
        Check.Equal(item.Status, RollingEngagementDigestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingEngagementDigestMovesToRunning()
    {
        var item = RollingEngagementDigest.NewRollingEngagementDigest("code-581", "label-491");
        try
        {
            item.MoveTo(RollingEngagementDigestStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingEngagementDigestStatus.Running, "status");
    }

    public static void RollingEngagementDigestRejectsMovesFromFailed()
    {
        var item = RollingEngagementDigest.NewRollingEngagementDigest("code-581", "label-491");
        item.Status = RollingEngagementDigestStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingEngagementDigestStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingEngagementDigestStartsInNew();
        RollingEngagementDigestMovesToRunning();
        RollingEngagementDigestRejectsMovesFromFailed();
    }
}
