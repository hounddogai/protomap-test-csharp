namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAuditChecks
{
    public static void SentimentAuditStartsInProposed()
    {
        var item = SentimentAudit.NewSentimentAudit("code-735", "label-38");
        Check.Equal(item.Status, SentimentAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentAuditMovesToApproved()
    {
        var item = SentimentAudit.NewSentimentAudit("code-735", "label-38");
        try
        {
            item.MoveTo(SentimentAuditStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentAuditStatus.Approved, "status");
    }

    public static void SentimentAuditRejectsMovesFromWithdrawn()
    {
        var item = SentimentAudit.NewSentimentAudit("code-735", "label-38");
        item.Status = SentimentAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SentimentAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SentimentAuditStartsInProposed();
        SentimentAuditMovesToApproved();
        SentimentAuditRejectsMovesFromWithdrawn();
    }
}
