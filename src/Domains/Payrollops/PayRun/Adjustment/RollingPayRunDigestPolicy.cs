namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling pay run digest policy.</summary>
public static class RollingPayRunDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31538.0, in order.</summary>
    public static int RollingPayRunDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31538.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a rolling pay run digest code in its canonical RPRD- form.</summary>
    public static string NormalizeRollingPayRunDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPRD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPRD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRollingPayRunDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
