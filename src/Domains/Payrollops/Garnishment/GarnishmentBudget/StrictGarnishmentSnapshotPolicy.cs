namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict garnishment snapshot policy.</summary>
public static class StrictGarnishmentSnapshotPolicy
{
    public static List<string> MergeStrictGarnishmentSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string StrictGarnishmentSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a strict garnishment snapshot code in its canonical SGS- form.</summary>
    public static string NormalizeStrictGarnishmentSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SGS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SGS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double StrictGarnishmentSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
