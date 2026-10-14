namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual holiday cycle policy.</summary>
public static class ManualHolidayCyclePolicy
{
    public static List<string> MergeManualHolidayCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a manual holiday cycle code in its canonical MHC- form.</summary>
    public static string NormalizeManualHolidayCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MHC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MHC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
