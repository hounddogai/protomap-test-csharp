namespace Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy occupancy draft policy.</summary>
public static class LegacyOccupancyDraftPolicy
{
    public static int LegacyOccupancyDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a legacy occupancy draft code in its canonical LOD- form.</summary>
    public static string NormalizeLegacyOccupancyDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LOD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LOD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLegacyOccupancyDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 19241.0, in order.</summary>
    public static int LegacyOccupancyDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19241.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
