namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool baseline policy.</summary>
public static class PoolBaselinePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 49072.0, in order.</summary>
    public static int PoolBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49072.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PoolBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergePoolBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a pool baseline code in its canonical PB- form.</summary>
    public static string NormalizePoolBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
