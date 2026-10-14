namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Functions of rolling rate assignment from window.</summary>
public static class RollingRateAssignmentFromWindow
{
    public static Dictionary<string, double> SummarizeVendorWindowsForAssignment(List<VendorWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<RollingRateAssignment> DeriveRollingRateAssignmentsFromWindow(List<VendorWindow> items)
    {
        var outValue = new List<RollingRateAssignment>();
        foreach (var item in items)
        {
            var derived = RollingRateAssignment.NewRollingRateAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRollingRateAssignment(RollingRateAssignment item)
    {
        return VendorWindowPolicy.NormalizeVendorWindowCode(item.Code);
    }
}
