namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment segment policy.</summary>
public static class GarnishmentSegmentPolicy
{
    public static double GarnishmentSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.05;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeGarnishmentSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string GarnishmentSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a garnishment segment code in its canonical GS- form.</summary>
    public static string NormalizeGarnishmentSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
