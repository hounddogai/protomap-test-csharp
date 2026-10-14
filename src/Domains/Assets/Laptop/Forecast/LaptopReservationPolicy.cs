namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop reservation policy.</summary>
public static class LaptopReservationPolicy
{
    /// <summary>Names the band a laptop reservation score falls in.</summary>
    public static string ClassifyLaptopReservation(double score)
    {
        if (score < 12.6)
        {
            return "watch";
        }
        if (score < 21.3)
        {
            return "medium";
        }
        return "high";
    }

    public static int LaptopReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Puts a laptop reservation code in its canonical LR- form.</summary>
    public static string NormalizeLaptopReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLaptopReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
