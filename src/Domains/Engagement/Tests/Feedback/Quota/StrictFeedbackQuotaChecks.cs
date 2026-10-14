namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictFeedbackQuotaChecks
{
    public static void StrictFeedbackQuotaStartsInOpen()
    {
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("code-716", "title-885");
        Check.Equal(item.Status, StrictFeedbackQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictFeedbackQuotaMovesToArchived()
    {
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("code-716", "title-885");
        try
        {
            item.MoveTo(StrictFeedbackQuotaStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictFeedbackQuotaStatus.Archived, "status");
    }

    public static void StrictFeedbackQuotaRejectsMovesFromArchived()
    {
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("code-716", "title-885");
        item.Status = StrictFeedbackQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictFeedbackQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictFeedbackQuotaStartsInOpen();
        StrictFeedbackQuotaMovesToArchived();
        StrictFeedbackQuotaRejectsMovesFromArchived();
    }
}
