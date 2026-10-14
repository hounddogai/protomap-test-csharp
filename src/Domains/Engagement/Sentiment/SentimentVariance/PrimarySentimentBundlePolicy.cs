namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary sentiment bundle policy.</summary>
public static class PrimarySentimentBundlePolicy
{
    /// <summary>Names the band a primary sentiment bundle score falls in.</summary>
    public static string ClassifyPrimarySentimentBundle(double score)
    {
        if (score < 55.1)
        {
            return "high";
        }
        if (score < 77.3)
        {
            return "ok";
        }
        if (score < 86.8)
        {
            return "elevated";
        }
        return "critical";
    }

    public static List<string> MergePrimarySentimentBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimarySentimentBundle()
    {
        var policy = Retry.DefaultPolicy("primary-sentiment-bundle");
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
}
