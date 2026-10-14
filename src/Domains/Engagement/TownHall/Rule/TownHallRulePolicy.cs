namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of town hall rule policy.</summary>
public static class TownHallRulePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTownHallRule()
    {
        var policy = Retry.DefaultPolicy("town-hall-rule");
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

    public static int TownHallRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a town hall rule score falls in.</summary>
    public static string ClassifyTownHallRule(double score)
    {
        if (score < 2.6)
        {
            return "watch";
        }
        if (score < 47.9)
        {
            return "ok";
        }
        if (score < 73.3)
        {
            return "medium";
        }
        if (score < 97.4)
        {
            return "minimal";
        }
        return "low";
    }

    public static List<string> MergeTownHallRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
