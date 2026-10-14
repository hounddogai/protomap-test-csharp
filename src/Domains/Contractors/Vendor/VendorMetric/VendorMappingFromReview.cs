namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Functions of vendor mapping from review.</summary>
public static class VendorMappingFromReview
{
    public static Dictionary<string, double> SummarizeRegionalTimesheetReviewsForMapping(List<RegionalTimesheetReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<VendorMapping> DeriveVendorMappingsFromReview(List<RegionalTimesheetReview> items)
    {
        var outValue = new List<VendorMapping>();
        foreach (var item in items)
        {
            var derived = VendorMapping.NewVendorMapping(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectVendorMapping(VendorMapping item)
    {
        return TieredTimesheetBudgetPolicy.TieredTimesheetBudgetWeight(6, true);
    }
}
