namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentRuleChecks
{
    public static void SentimentRuleStartsInOpen()
    {
        var item = SentimentRule.NewSentimentRule("code-271", "name-872");
        Check.Equal(item.Status, SentimentRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentRuleMovesToArchived()
    {
        var item = SentimentRule.NewSentimentRule("code-271", "name-872");
        try
        {
            item.MoveTo(SentimentRuleStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentRuleStatus.Archived, "status");
    }

    public static void SentimentRuleRejectsMovesFromArchived()
    {
        var item = SentimentRule.NewSentimentRule("code-271", "name-872");
        item.Status = SentimentRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(SentimentRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        SentimentRuleStartsInOpen();
        SentimentRuleMovesToArchived();
        SentimentRuleRejectsMovesFromArchived();
    }
}
