namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled pay band review policy.</summary>
public static class ScheduledPayBandReviewPolicy
{
    public static List<string> MergeScheduledPayBandReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a scheduled pay band review code in its canonical SPBR- form.</summary>
    public static string NormalizeScheduledPayBandReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled pay band review score falls in.</summary>
    public static string ClassifyScheduledPayBandReview(double score)
    {
        if (score < 60.1)
        {
            return "high";
        }
        if (score < 67.1)
        {
            return "critical";
        }
        if (score < 83.1)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPayBandReview()
    {
        var policy = Retry.DefaultPolicy("scheduled-pay-band-review");
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
}
