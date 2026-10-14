namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackQuotaChecks
{
    public static void FeedbackQuotaStartsInCreated()
    {
        var item = FeedbackQuota.NewFeedbackQuota("code-86", "name-882");
        Check.Equal(item.Status, FeedbackQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackQuotaMovesToPublished()
    {
        var item = FeedbackQuota.NewFeedbackQuota("code-86", "name-882");
        try
        {
            item.MoveTo(FeedbackQuotaStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackQuotaStatus.Published, "status");
    }

    public static void FeedbackQuotaRejectsMovesFromRetired()
    {
        var item = FeedbackQuota.NewFeedbackQuota("code-86", "name-882");
        item.Status = FeedbackQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(FeedbackQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        FeedbackQuotaStartsInCreated();
        FeedbackQuotaMovesToPublished();
        FeedbackQuotaRejectsMovesFromRetired();
    }
}
