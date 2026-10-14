namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor cycle policy.</summary>
public static class VisitorCyclePolicy
{
    public static List<string> MergeVisitorCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorCycle()
    {
        var policy = Retry.DefaultPolicy("visitor-cycle");
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

    /// <summary>Names the band a visitor cycle score falls in.</summary>
    public static string ClassifyVisitorCycle(double score)
    {
        if (score < 65.2)
        {
            return "ok";
        }
        if (score < 69.4)
        {
            return "medium";
        }
        if (score < 79.9)
        {
            return "critical";
        }
        if (score < 84.3)
        {
            return "minimal";
        }
        return "watch";
    }
}
