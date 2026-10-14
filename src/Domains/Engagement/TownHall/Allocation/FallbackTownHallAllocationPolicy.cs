namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback town hall allocation policy.</summary>
public static class FallbackTownHallAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 69454.0, in order.</summary>
    public static int FallbackTownHallAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69454.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a fallback town hall allocation score falls in.</summary>
    public static string ClassifyFallbackTownHallAllocation(double score)
    {
        if (score < 10.8)
        {
            return "watch";
        }
        if (score < 36.1)
        {
            return "ok";
        }
        if (score < 99.9)
        {
            return "critical";
        }
        return "medium";
    }

    public static List<string> MergeFallbackTownHallAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a fallback town hall allocation code in its canonical FTHA- form.</summary>
    public static string NormalizeFallbackTownHallAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FTHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FTHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
