namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay band review policy.</summary>
public static class PayBandReviewPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandReview()
    {
        var policy = Retry.DefaultPolicy("pay-band-review");
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

    /// <summary>Names the band a pay band review score falls in.</summary>
    public static string ClassifyPayBandReview(double score)
    {
        if (score < 3.0)
        {
            return "minimal";
        }
        if (score < 26.7)
        {
            return "high";
        }
        if (score < 45.0)
        {
            return "ok";
        }
        if (score < 85.0)
        {
            return "elevated";
        }
        return "medium";
    }

    public static List<string> MergePayBandReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
