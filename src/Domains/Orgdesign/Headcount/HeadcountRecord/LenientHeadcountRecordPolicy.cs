namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient headcount record policy.</summary>
public static class LenientHeadcountRecordPolicy
{
    /// <summary>Puts a lenient headcount record code in its canonical LHR- form.</summary>
    public static string NormalizeLenientHeadcountRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LHR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LHR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientHeadcountRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static List<string> MergeLenientHeadcountRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
