namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty bundle policy.</summary>
public static class WarrantyBundlePolicy
{
    /// <summary>Names the band a warranty bundle score falls in.</summary>
    public static string ClassifyWarrantyBundle(double score)
    {
        if (score < 5.6)
        {
            return "minimal";
        }
        if (score < 43.4)
        {
            return "low";
        }
        if (score < 89.9)
        {
            return "watch";
        }
        return "elevated";
    }

    public static int WarrantyBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeWarrantyBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
