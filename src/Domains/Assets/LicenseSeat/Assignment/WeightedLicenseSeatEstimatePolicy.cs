namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted license seat estimate policy.</summary>
public static class WeightedLicenseSeatEstimatePolicy
{
    public static string WeightedLicenseSeatEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeWeightedLicenseSeatEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedLicenseSeatEstimate()
    {
        var policy = Retry.DefaultPolicy("weighted-license-seat-estimate");
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

    /// <summary>Names the band a weighted license seat estimate score falls in.</summary>
    public static string ClassifyWeightedLicenseSeatEstimate(double score)
    {
        if (score < 19.6)
        {
            return "elevated";
        }
        if (score < 27.3)
        {
            return "medium";
        }
        if (score < 43.1)
        {
            return "minimal";
        }
        return "high";
    }
}
