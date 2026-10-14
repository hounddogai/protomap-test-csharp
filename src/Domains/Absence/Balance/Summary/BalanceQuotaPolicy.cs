namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of balance quota policy.</summary>
public static class BalanceQuotaPolicy
{
    public static int BalanceQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBalanceQuota()
    {
        var policy = Retry.DefaultPolicy("balance-quota");
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

    public static List<string> MergeBalanceQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a balance quota score falls in.</summary>
    public static string ClassifyBalanceQuota(double score)
    {
        if (score < 3.6)
        {
            return "elevated";
        }
        if (score < 6.9)
        {
            return "high";
        }
        if (score < 74.0)
        {
            return "low";
        }
        return "ok";
    }
}
