namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default leave request item policy.</summary>
public static class DefaultLeaveRequestItemPolicy
{
    public static string DefaultLeaveRequestItemBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeDefaultLeaveRequestItemLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a default leave request item code in its canonical DLRI- form.</summary>
    public static string NormalizeDefaultLeaveRequestItemCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DLRI-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DLRI-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
