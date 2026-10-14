namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled desk rollup policy.</summary>
public static class ScheduledDeskRollupPolicy
{
    public static List<string> MergeScheduledDeskRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled desk rollup score falls in.</summary>
    public static string ClassifyScheduledDeskRollup(double score)
    {
        if (score < 18.5)
        {
            return "high";
        }
        if (score < 27.7)
        {
            return "elevated";
        }
        if (score < 89.6)
        {
            return "watch";
        }
        if (score < 97.7)
        {
            return "low";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledDeskRollup()
    {
        var policy = Retry.DefaultPolicy("scheduled-desk-rollup");
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
