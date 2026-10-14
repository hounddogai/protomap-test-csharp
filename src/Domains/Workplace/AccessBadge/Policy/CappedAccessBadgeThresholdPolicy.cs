namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped access badge threshold policy.</summary>
public static class CappedAccessBadgeThresholdPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedAccessBadgeThreshold()
    {
        var policy = Retry.DefaultPolicy("capped-access-badge-threshold");
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

    public static List<string> MergeCappedAccessBadgeThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double CappedAccessBadgeThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
