namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of retro pay rollup policy.</summary>
public static class RetroPayRollupPolicy
{
    /// <summary>Names the band a retro pay rollup score falls in.</summary>
    public static string ClassifyRetroPayRollup(double score)
    {
        if (score < 51.8)
        {
            return "high";
        }
        if (score < 60.4)
        {
            return "low";
        }
        return "critical";
    }

    public static List<string> MergeRetroPayRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRetroPayRollup()
    {
        var policy = Retry.DefaultPolicy("retro-pay-rollup");
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

    public static string RetroPayRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
