namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual vendor milestone policy.</summary>
public static class ManualVendorMilestonePolicy
{
    public static List<string> MergeManualVendorMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a manual vendor milestone score falls in.</summary>
    public static string ClassifyManualVendorMilestone(double score)
    {
        if (score < 12.2)
        {
            return "low";
        }
        if (score < 70.8)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Puts a manual vendor milestone code in its canonical MVM- form.</summary>
    public static string NormalizeManualVendorMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MVM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MVM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
