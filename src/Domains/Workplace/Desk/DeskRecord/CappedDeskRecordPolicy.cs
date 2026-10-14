namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped desk record policy.</summary>
public static class CappedDeskRecordPolicy
{
    public static int CappedDeskRecordSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a capped desk record code in its canonical CDR- form.</summary>
    public static string NormalizeCappedDeskRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CDR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CDR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCappedDeskRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
