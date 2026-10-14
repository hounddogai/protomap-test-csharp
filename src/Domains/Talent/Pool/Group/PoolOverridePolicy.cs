namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool override policy.</summary>
public static class PoolOverridePolicy
{
    /// <summary>Puts a pool override code in its canonical PO- form.</summary>
    public static string NormalizePoolOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePoolOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 4416.0, in order.</summary>
    public static int PoolOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4416.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
