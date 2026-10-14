namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan ledger policy.</summary>
public static class SuccessionPlanLedgerPolicy
{
    public static List<string> MergeSuccessionPlanLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSuccessionPlanLedger()
    {
        var policy = Retry.DefaultPolicy("succession-plan-ledger");
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

    /// <summary>Puts a succession plan ledger code in its canonical SPL- form.</summary>
    public static string NormalizeSuccessionPlanLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a succession plan ledger score falls in.</summary>
    public static string ClassifySuccessionPlanLedger(double score)
    {
        if (score < 21.3)
        {
            return "ok";
        }
        if (score < 37.7)
        {
            return "high";
        }
        if (score < 60.9)
        {
            return "medium";
        }
        if (score < 74.4)
        {
            return "critical";
        }
        return "low";
    }
}
