namespace Acme.Workforce.Contractors.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

/// <summary>Functions of vendor score from schedule.</summary>
public static class VendorScoreFromSchedule
{
    public static Dictionary<string, double> SummarizeRegionalVendorSchedulesForScore(List<RegionalVendorSchedule> items)
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

    public static List<VendorScore> DeriveVendorScoresFromSchedule(List<RegionalVendorSchedule> items)
    {
        var outValue = new List<VendorScore>();
        foreach (var item in items)
        {
            var derived = VendorScore.NewVendorScore(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileVendorScore(VendorScore item)
    {
        return VendorProfilePolicy.VendorProfileWeight(4, true);
    }
}
