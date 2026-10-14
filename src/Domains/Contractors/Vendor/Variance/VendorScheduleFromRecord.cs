namespace Acme.Workforce.Contractors.Vendor.Variance;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Functions of vendor schedule from record.</summary>
public static class VendorScheduleFromRecord
{
    public static Dictionary<string, double> SummarizeRateRecordsForSchedule(List<RateRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<VendorSchedule> DeriveVendorSchedulesFromRecord(List<RateRecord> items)
    {
        var outValue = new List<VendorSchedule>();
        foreach (var item in items)
        {
            var derived = VendorSchedule.NewVendorSchedule(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveVendorSchedule(VendorSchedule item)
    {
        return RateProposalPolicy.RateProposalWeight(6, true);
    }
}
