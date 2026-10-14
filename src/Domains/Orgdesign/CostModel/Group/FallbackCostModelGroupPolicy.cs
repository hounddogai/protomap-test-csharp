namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback cost model group policy.</summary>
public static class FallbackCostModelGroupPolicy
{
    /// <summary>Puts a fallback cost model group code in its canonical FCMG- form.</summary>
    public static string NormalizeFallbackCostModelGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FCMG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FCMG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeFallbackCostModelGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 88749.0, in order.</summary>
    public static int FallbackCostModelGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88749.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int FallbackCostModelGroupSpan(int start, int end)
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
