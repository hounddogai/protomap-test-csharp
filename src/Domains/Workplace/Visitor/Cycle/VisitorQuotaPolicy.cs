namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor quota policy.</summary>
public static class VisitorQuotaPolicy
{
    /// <summary>Puts a visitor quota code in its canonical VQ- form.</summary>
    public static string NormalizeVisitorQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeVisitorQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 49156.0, in order.</summary>
    public static int VisitorQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49156.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
