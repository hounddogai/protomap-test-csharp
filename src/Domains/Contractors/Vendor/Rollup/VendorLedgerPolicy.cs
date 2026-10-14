namespace Acme.Workforce.Contractors.Vendor.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of vendor ledger policy.</summary>
public static class VendorLedgerPolicy
{
    /// <summary>Names the band a vendor ledger score falls in.</summary>
    public static string ClassifyVendorLedger(double score)
    {
        if (score < 14.0)
        {
            return "critical";
        }
        if (score < 55.9)
        {
            return "ok";
        }
        if (score < 72.1)
        {
            return "elevated";
        }
        if (score < 72.9)
        {
            return "low";
        }
        return "high";
    }

    public static List<string> MergeVendorLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
