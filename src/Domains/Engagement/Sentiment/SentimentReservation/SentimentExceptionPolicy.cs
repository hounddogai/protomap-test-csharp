namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment exception policy.</summary>
public static class SentimentExceptionPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentException()
    {
        var policy = Retry.DefaultPolicy("sentiment-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a sentiment exception score falls in.</summary>
    public static string ClassifySentimentException(double score)
    {
        if (score < 2.8)
        {
            return "low";
        }
        if (score < 27.7)
        {
            return "watch";
        }
        if (score < 76.1)
        {
            return "ok";
        }
        return "minimal";
    }

    public static string SentimentExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int SentimentExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
