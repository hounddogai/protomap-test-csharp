namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending town hall baseline policy.</summary>
public static class PendingTownHallBaselinePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingTownHallBaseline()
    {
        var policy = Retry.DefaultPolicy("pending-town-hall-baseline");
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

    /// <summary>Names the band a pending town hall baseline score falls in.</summary>
    public static string ClassifyPendingTownHallBaseline(double score)
    {
        if (score < 38.9)
        {
            return "ok";
        }
        if (score < 44.8)
        {
            return "low";
        }
        if (score < 48.7)
        {
            return "critical";
        }
        if (score < 95.7)
        {
            return "watch";
        }
        return "medium";
    }

    public static List<string> MergePendingTownHallBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
