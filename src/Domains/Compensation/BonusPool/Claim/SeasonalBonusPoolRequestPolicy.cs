namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal bonus pool request policy.</summary>
public static class SeasonalBonusPoolRequestPolicy
{
    /// <summary>Names the band a seasonal bonus pool request score falls in.</summary>
    public static string ClassifySeasonalBonusPoolRequest(double score)
    {
        if (score < 25.9)
        {
            return "elevated";
        }
        if (score < 81.0)
        {
            return "medium";
        }
        if (score < 92.7)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeSeasonalBonusPoolRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double SeasonalBonusPoolRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalBonusPoolRequest()
    {
        var policy = Retry.DefaultPolicy("seasonal-bonus-pool-request");
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
