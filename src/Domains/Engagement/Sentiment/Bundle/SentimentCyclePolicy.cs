namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of sentiment cycle policy.</summary>
public static class SentimentCyclePolicy
{
    public static string SentimentCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSentimentCycle()
    {
        var policy = Retry.DefaultPolicy("sentiment-cycle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a sentiment cycle code in its canonical SC- form.</summary>
    public static string NormalizeSentimentCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
