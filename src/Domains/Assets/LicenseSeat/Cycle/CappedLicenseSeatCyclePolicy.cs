namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped license seat cycle policy.</summary>
public static class CappedLicenseSeatCyclePolicy
{
    /// <summary>Puts a capped license seat cycle code in its canonical CLSC- form.</summary>
    public static string NormalizeCappedLicenseSeatCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CLSC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CLSC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CappedLicenseSeatCycleSpan(int start, int end)
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
        return span / 3600;
    }

    public static List<string> MergeCappedLicenseSeatCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double CappedLicenseSeatCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
