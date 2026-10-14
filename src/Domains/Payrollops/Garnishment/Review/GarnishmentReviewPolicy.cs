namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment review policy.</summary>
public static class GarnishmentReviewPolicy
{
    /// <summary>Puts a garnishment review code in its canonical GR- form.</summary>
    public static string NormalizeGarnishmentReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a garnishment review score falls in.</summary>
    public static string ClassifyGarnishmentReview(double score)
    {
        if (score < 15.9)
        {
            return "high";
        }
        if (score < 68.7)
        {
            return "critical";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGarnishmentReview()
    {
        var policy = Retry.DefaultPolicy("garnishment-review");
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

    public static List<string> MergeGarnishmentReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
