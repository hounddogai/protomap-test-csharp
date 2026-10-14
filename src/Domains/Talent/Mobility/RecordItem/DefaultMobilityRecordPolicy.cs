namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default mobility record policy.</summary>
public static class DefaultMobilityRecordPolicy
{
    public static double DefaultMobilityRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.92;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a default mobility record code in its canonical DMR- form.</summary>
    public static string NormalizeDefaultMobilityRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeDefaultMobilityRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
