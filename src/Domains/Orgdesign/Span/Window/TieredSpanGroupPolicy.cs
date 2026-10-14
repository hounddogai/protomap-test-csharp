namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered span group policy.</summary>
public static class TieredSpanGroupPolicy
{
    public static List<string> MergeTieredSpanGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a tiered span group code in its canonical TSG- form.</summary>
    public static string NormalizeTieredSpanGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TSG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TSG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TieredSpanGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
