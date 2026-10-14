namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor plan policy.</summary>
public static class VisitorPlanPolicy
{
    public static List<string> MergeVisitorPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a visitor plan score falls in.</summary>
    public static string ClassifyVisitorPlan(double score)
    {
        if (score < 13.5)
        {
            return "high";
        }
        if (score < 29.2)
        {
            return "minimal";
        }
        if (score < 86.8)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorPlan()
    {
        var policy = Retry.DefaultPolicy("visitor-plan");
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
}
