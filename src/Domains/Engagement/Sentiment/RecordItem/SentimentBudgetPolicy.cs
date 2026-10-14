namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment budget policy.</summary>
public static class SentimentBudgetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentBudget()
    {
        var policy = Retry.DefaultPolicy("sentiment-budget");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int SentimentBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a sentiment budget score falls in.</summary>
    public static string ClassifySentimentBudget(double score)
    {
        if (score < 44.9)
        {
            return "medium";
        }
        if (score < 60.1)
        {
            return "low";
        }
        if (score < 64.9)
        {
            return "minimal";
        }
        return "critical";
    }
}
