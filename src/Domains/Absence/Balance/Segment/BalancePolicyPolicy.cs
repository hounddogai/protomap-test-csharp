namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of balance policy policy.</summary>
public static class BalancePolicyPolicy
{
    public static int BalancePolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBalancePolicy()
    {
        var policy = Retry.DefaultPolicy("balance-policy");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeBalancePolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a balance policy score falls in.</summary>
    public static string ClassifyBalancePolicy(double score)
    {
        if (score < 56.1)
        {
            return "minimal";
        }
        if (score < 72.0)
        {
            return "medium";
        }
        if (score < 73.1)
        {
            return "watch";
        }
        return "critical";
    }
}
