namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit cycle policy.</summary>
public static class AuditCyclePolicy
{
    public static List<string> MergeAuditCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string AuditCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a audit cycle code in its canonical AC- form.</summary>
    public static string NormalizeAuditCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
