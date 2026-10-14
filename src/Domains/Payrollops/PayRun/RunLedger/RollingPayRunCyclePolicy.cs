namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling pay run cycle policy.</summary>
public static class RollingPayRunCyclePolicy
{
    public static List<string> MergeRollingPayRunCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a rolling pay run cycle code in its canonical RPRC- form.</summary>
    public static string NormalizeRollingPayRunCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RollingPayRunCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
