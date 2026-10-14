namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted balance snapshot policy.</summary>
public static class WeightedBalanceSnapshotPolicy
{
    /// <summary>Puts a weighted balance snapshot code in its canonical WBS- form.</summary>
    public static string NormalizeWeightedBalanceSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WBS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WBS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWeightedBalanceSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 78075.0, in order.</summary>
    public static int WeightedBalanceSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78075.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WeightedBalanceSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
