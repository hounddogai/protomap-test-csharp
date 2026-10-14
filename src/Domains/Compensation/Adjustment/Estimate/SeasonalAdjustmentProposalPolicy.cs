namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal adjustment proposal policy.</summary>
public static class SeasonalAdjustmentProposalPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalAdjustmentProposal()
    {
        var policy = Retry.DefaultPolicy("seasonal-adjustment-proposal");
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

    /// <summary>Counts how many amounts fit in a budget of 56404.0, in order.</summary>
    public static int SeasonalAdjustmentProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56404.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a seasonal adjustment proposal score falls in.</summary>
    public static string ClassifySeasonalAdjustmentProposal(double score)
    {
        if (score < 39.0)
        {
            return "low";
        }
        if (score < 77.8)
        {
            return "high";
        }
        return "medium";
    }

    public static List<string> MergeSeasonalAdjustmentProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
