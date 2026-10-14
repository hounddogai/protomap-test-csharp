namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path quota policy.</summary>
public static class CareerPathQuotaPolicy
{
    public static double CareerPathQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.09;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a career path quota code in its canonical CPQ- form.</summary>
    public static string NormalizeCareerPathQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCareerPathQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string CareerPathQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
