namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted audit ledger policy.</summary>
public static class WeightedAuditLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAuditLedger()
    {
        var policy = Retry.DefaultPolicy("weighted-audit-ledger");
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

    public static string WeightedAuditLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a weighted audit ledger score falls in.</summary>
    public static string ClassifyWeightedAuditLedger(double score)
    {
        if (score < 19.6)
        {
            return "minimal";
        }
        if (score < 31.5)
        {
            return "elevated";
        }
        if (score < 78.2)
        {
            return "medium";
        }
        if (score < 81.2)
        {
            return "watch";
        }
        return "high";
    }

    public static List<string> MergeWeightedAuditLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
