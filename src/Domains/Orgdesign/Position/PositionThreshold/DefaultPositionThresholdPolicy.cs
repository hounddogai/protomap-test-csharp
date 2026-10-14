namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default position threshold policy.</summary>
public static class DefaultPositionThresholdPolicy
{
    /// <summary>Names the band a default position threshold score falls in.</summary>
    public static string ClassifyDefaultPositionThreshold(double score)
    {
        if (score < 2.6)
        {
            return "ok";
        }
        if (score < 65.4)
        {
            return "critical";
        }
        if (score < 99.1)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultPositionThreshold()
    {
        var policy = Retry.DefaultPolicy("default-position-threshold");
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

    public static string DefaultPositionThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeDefaultPositionThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
