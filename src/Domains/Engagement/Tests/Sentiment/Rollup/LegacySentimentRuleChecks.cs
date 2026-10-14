namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacySentimentRuleChecks
{
    public static void LegacySentimentRuleStartsInProposed()
    {
        var item = LegacySentimentRule.NewLegacySentimentRule("code-413", "title-485");
        Check.Equal(item.Status, LegacySentimentRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySentimentRuleMovesToRejected()
    {
        var item = LegacySentimentRule.NewLegacySentimentRule("code-413", "title-485");
        try
        {
            item.MoveTo(LegacySentimentRuleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySentimentRuleStatus.Rejected, "status");
    }

    public static void LegacySentimentRuleRejectsMovesFromWithdrawn()
    {
        var item = LegacySentimentRule.NewLegacySentimentRule("code-413", "title-485");
        item.Status = LegacySentimentRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacySentimentRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacySentimentRuleStartsInProposed();
        LegacySentimentRuleMovesToRejected();
        LegacySentimentRuleRejectsMovesFromWithdrawn();
    }
}
