namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback visitor cycle policy.</summary>
public static class FallbackVisitorCyclePolicy
{
    public static List<string> MergeFallbackVisitorCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string FallbackVisitorCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int FallbackVisitorCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a fallback visitor cycle code in its canonical FVC- form.</summary>
    public static string NormalizeFallbackVisitorCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FVC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FVC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
