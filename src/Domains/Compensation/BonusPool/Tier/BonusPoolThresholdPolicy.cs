namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool threshold policy.</summary>
public static class BonusPoolThresholdPolicy
{
    /// <summary>Puts a bonus pool threshold code in its canonical BPT- form.</summary>
    public static string NormalizeBonusPoolThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 5474.0, in order.</summary>
    public static int BonusPoolThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5474.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeBonusPoolThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int BonusPoolThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
