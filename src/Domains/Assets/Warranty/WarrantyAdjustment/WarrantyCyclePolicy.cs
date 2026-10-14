namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty cycle policy.</summary>
public static class WarrantyCyclePolicy
{
    public static double WarrantyCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeWarrantyCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a warranty cycle score falls in.</summary>
    public static string ClassifyWarrantyCycle(double score)
    {
        if (score < 12.2)
        {
            return "elevated";
        }
        if (score < 89.2)
        {
            return "minimal";
        }
        return "critical";
    }
}
