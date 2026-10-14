namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy garnishment allocation policy.</summary>
public static class LegacyGarnishmentAllocationPolicy
{
    /// <summary>Puts a legacy garnishment allocation code in its canonical LGA- form.</summary>
    public static string NormalizeLegacyGarnishmentAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LGA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LGA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LegacyGarnishmentAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeLegacyGarnishmentAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string LegacyGarnishmentAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
