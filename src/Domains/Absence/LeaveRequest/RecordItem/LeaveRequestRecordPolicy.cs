namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of leave request record policy.</summary>
public static class LeaveRequestRecordPolicy
{
    /// <summary>Puts a leave request record code in its canonical LRR- form.</summary>
    public static string NormalizeLeaveRequestRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLeaveRequestRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
