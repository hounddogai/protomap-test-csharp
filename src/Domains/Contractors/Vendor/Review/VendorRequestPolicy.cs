namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of vendor request policy.</summary>
public static class VendorRequestPolicy
{
    /// <summary>Names the band a vendor request score falls in.</summary>
    public static string ClassifyVendorRequest(double score)
    {
        if (score < 6.0)
        {
            return "medium";
        }
        if (score < 66.3)
        {
            return "high";
        }
        if (score < 79.0)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeVendorRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
