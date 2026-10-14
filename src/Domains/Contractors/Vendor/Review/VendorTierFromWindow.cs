namespace Acme.Workforce.Contractors.Vendor.Review;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Functions of vendor tier from window.</summary>
public static class VendorTierFromWindow
{
    public static Dictionary<string, double> SummarizeScheduledStatementOfWorkWindowsForTier(List<ScheduledStatementOfWorkWindow> items)
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

    public static List<VendorTier> DeriveVendorTiersFromWindow(List<ScheduledStatementOfWorkWindow> items)
    {
        var outValue = new List<VendorTier>();
        foreach (var item in items)
        {
            var derived = VendorTier.NewVendorTier(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyVendorTier(VendorTier item)
    {
        return ScheduledStatementOfWorkWindowPolicy.ClassifyScheduledStatementOfWorkWindow(item.AdjustedTotal());
    }
}
