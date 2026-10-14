namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling reorg summary policy.</summary>
public static class RollingReorgSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72986.0, in order.</summary>
    public static int RollingReorgSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72986.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeRollingReorgSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingReorgSummary()
    {
        var policy = Retry.DefaultPolicy("rolling-reorg-summary");
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

    /// <summary>Names the band a rolling reorg summary score falls in.</summary>
    public static string ClassifyRollingReorgSummary(double score)
    {
        if (score < 28.9)
        {
            return "low";
        }
        if (score < 53.9)
        {
            return "watch";
        }
        if (score < 72.5)
        {
            return "high";
        }
        if (score < 74.6)
        {
            return "elevated";
        }
        return "critical";
    }
}
