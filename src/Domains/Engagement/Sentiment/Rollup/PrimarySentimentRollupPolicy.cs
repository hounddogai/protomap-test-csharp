namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary sentiment rollup policy.</summary>
public static class PrimarySentimentRollupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimarySentimentRollup()
    {
        var policy = Retry.DefaultPolicy("primary-sentiment-rollup");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a primary sentiment rollup score falls in.</summary>
    public static string ClassifyPrimarySentimentRollup(double score)
    {
        if (score < 52.5)
        {
            return "high";
        }
        if (score < 93.4)
        {
            return "watch";
        }
        return "minimal";
    }

    public static List<string> MergePrimarySentimentRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a primary sentiment rollup code in its canonical PSR- form.</summary>
    public static string NormalizePrimarySentimentRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
