namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of cost model estimate policy.</summary>
public static class CostModelEstimatePolicy
{
    public static List<string> MergeCostModelEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string CostModelEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelEstimate()
    {
        var policy = Retry.DefaultPolicy("cost-model-estimate");
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

    /// <summary>Names the band a cost model estimate score falls in.</summary>
    public static string ClassifyCostModelEstimate(double score)
    {
        if (score < 16.5)
        {
            return "medium";
        }
        if (score < 24.6)
        {
            return "ok";
        }
        if (score < 55.6)
        {
            return "minimal";
        }
        if (score < 81.5)
        {
            return "watch";
        }
        return "low";
    }
}
