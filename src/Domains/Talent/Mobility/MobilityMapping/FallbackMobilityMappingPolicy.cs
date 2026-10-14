namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback mobility mapping policy.</summary>
public static class FallbackMobilityMappingPolicy
{
    public static List<string> MergeFallbackMobilityMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 53506.0, in order.</summary>
    public static int FallbackMobilityMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53506.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int FallbackMobilityMappingSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a fallback mobility mapping code in its canonical FMM- form.</summary>
    public static string NormalizeFallbackMobilityMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FMM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FMM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
