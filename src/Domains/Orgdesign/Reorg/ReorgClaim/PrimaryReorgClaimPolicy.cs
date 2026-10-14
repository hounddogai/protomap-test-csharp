namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary reorg claim policy.</summary>
public static class PrimaryReorgClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 53776.0, in order.</summary>
    public static int PrimaryReorgClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53776.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a primary reorg claim score falls in.</summary>
    public static string ClassifyPrimaryReorgClaim(double score)
    {
        if (score < 49.8)
        {
            return "medium";
        }
        if (score < 82.6)
        {
            return "low";
        }
        if (score < 86.0)
        {
            return "watch";
        }
        return "elevated";
    }

    public static List<string> MergePrimaryReorgClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
