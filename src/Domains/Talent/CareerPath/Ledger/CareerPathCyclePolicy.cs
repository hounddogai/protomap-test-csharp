namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path cycle policy.</summary>
public static class CareerPathCyclePolicy
{
    /// <summary>Names the band a career path cycle score falls in.</summary>
    public static string ClassifyCareerPathCycle(double score)
    {
        if (score < 23.3)
        {
            return "medium";
        }
        if (score < 32.3)
        {
            return "elevated";
        }
        if (score < 53.5)
        {
            return "watch";
        }
        if (score < 92.2)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathCycle()
    {
        var policy = Retry.DefaultPolicy("career-path-cycle");
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

    public static double CareerPathCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeCareerPathCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
