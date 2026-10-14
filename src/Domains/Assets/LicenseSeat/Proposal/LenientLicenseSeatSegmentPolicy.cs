namespace Acme.Workforce.Assets.LicenseSeat.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient license seat segment policy.</summary>
public static class LenientLicenseSeatSegmentPolicy
{
    public static double LenientLicenseSeatSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LenientLicenseSeatSegmentSpan(int start, int end)
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

    public static List<string> MergeLenientLicenseSeatSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a lenient license seat segment code in its canonical LLSS- form.</summary>
    public static string NormalizeLenientLicenseSeatSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
