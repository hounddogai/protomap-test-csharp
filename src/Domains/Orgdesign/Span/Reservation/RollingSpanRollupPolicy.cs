namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling span rollup policy.</summary>
public static class RollingSpanRollupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingSpanRollup()
    {
        var policy = Retry.DefaultPolicy("rolling-span-rollup");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string RollingSpanRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a rolling span rollup score falls in.</summary>
    public static string ClassifyRollingSpanRollup(double score)
    {
        if (score < 33.4)
        {
            return "watch";
        }
        if (score < 48.0)
        {
            return "elevated";
        }
        if (score < 67.4)
        {
            return "ok";
        }
        return "low";
    }

    public static List<string> MergeRollingSpanRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
