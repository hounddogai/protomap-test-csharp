namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of access badge milestone policy.</summary>
public static class AccessBadgeMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 62853.0, in order.</summary>
    public static int AccessBadgeMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62853.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAccessBadgeMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int AccessBadgeMilestoneSpan(int start, int end)
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

    /// <summary>Puts a access badge milestone code in its canonical ABM- form.</summary>
    public static string NormalizeAccessBadgeMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ABM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ABM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
