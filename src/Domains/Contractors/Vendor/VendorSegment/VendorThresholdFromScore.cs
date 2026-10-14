namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Functions of vendor threshold from score.</summary>
public static class VendorThresholdFromScore
{
    public static Dictionary<string, double> SummarizeVendorScoresForThreshold(List<VendorScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<VendorThreshold> DeriveVendorThresholdsFromScore(List<VendorScore> items)
    {
        var outValue = new List<VendorThreshold>();
        foreach (var item in items)
        {
            var derived = VendorThreshold.NewVendorThreshold(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeVendorThreshold(VendorThreshold item)
    {
        return AdaptiveVendorAssignmentPolicy.AdaptiveVendorAssignmentWeight(5, true);
    }
}
