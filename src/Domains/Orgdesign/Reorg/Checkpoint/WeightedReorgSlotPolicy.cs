namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted reorg slot policy.</summary>
public static class WeightedReorgSlotPolicy
{
    public static string WeightedReorgSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a weighted reorg slot code in its canonical WRS- form.</summary>
    public static string NormalizeWeightedReorgSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeWeightedReorgSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 1239.0, in order.</summary>
    public static int WeightedReorgSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1239.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
