namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor mapping policy.</summary>
public static class VisitorMappingPolicy
{
    /// <summary>Puts a visitor mapping code in its canonical VM- form.</summary>
    public static string NormalizeVisitorMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int VisitorMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    public static List<string> MergeVisitorMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 79490.0, in order.</summary>
    public static int VisitorMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79490.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
