namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk segment policy.</summary>
public static class DeskSegmentPolicy
{
    public static int DeskSegmentSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Puts a desk segment code in its canonical DS- form.</summary>
    public static string NormalizeDeskSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDeskSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
