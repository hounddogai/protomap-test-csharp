namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped offboarding schedule policy.</summary>
public static class CappedOffboardingSchedulePolicy
{
    /// <summary>Puts a capped offboarding schedule code in its canonical COS- form.</summary>
    public static string NormalizeCappedOffboardingScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("COS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"COS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCappedOffboardingScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
