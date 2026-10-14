namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global vendor review policy.</summary>
public static class GlobalVendorReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 4485.0, in order.</summary>
    public static int GlobalVendorReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 4485.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeGlobalVendorReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a global vendor review code in its canonical GVR- form.</summary>
    public static string NormalizeGlobalVendorReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GVR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GVR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
