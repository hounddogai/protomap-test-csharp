namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of accrual target policy.</summary>
public static class AccrualTargetPolicy
{
    public static List<string> MergeAccrualTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a accrual target code in its canonical AT- form.</summary>
    public static string NormalizeAccrualTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
