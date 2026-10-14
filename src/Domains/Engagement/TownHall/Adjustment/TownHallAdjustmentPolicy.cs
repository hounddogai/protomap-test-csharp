namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of town hall adjustment policy.</summary>
public static class TownHallAdjustmentPolicy
{
    public static string TownHallAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTownHallAdjustment()
    {
        var policy = Retry.DefaultPolicy("town-hall-adjustment");
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

    public static List<string> MergeTownHallAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a town hall adjustment score falls in.</summary>
    public static string ClassifyTownHallAdjustment(double score)
    {
        if (score < 37.7)
        {
            return "minimal";
        }
        if (score < 71.7)
        {
            return "critical";
        }
        if (score < 77.0)
        {
            return "ok";
        }
        return "medium";
    }
}
