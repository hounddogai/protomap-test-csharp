namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of leave request tier policy.</summary>
public static class LeaveRequestTierPolicy
{
    public static List<string> MergeLeaveRequestTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a leave request tier code in its canonical LRT- form.</summary>
    public static string NormalizeLeaveRequestTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LeaveRequestTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
