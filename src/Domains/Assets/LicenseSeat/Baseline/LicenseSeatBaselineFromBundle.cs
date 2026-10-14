namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Assets.Warranty.Cycle;

/// <summary>Functions of license seat baseline from bundle.</summary>
public static class LicenseSeatBaselineFromBundle
{
    public static Dictionary<string, double> SummarizeBatchedWarrantyBundlesForBaseline(List<BatchedWarrantyBundle> items)
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

    public static List<LicenseSeatBaseline> DeriveLicenseSeatBaselinesFromBundle(List<BatchedWarrantyBundle> items)
    {
        var outValue = new List<LicenseSeatBaseline>();
        foreach (var item in items)
        {
            var derived = LicenseSeatBaseline.NewLicenseSeatBaseline(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyLicenseSeatBaseline(LicenseSeatBaseline item)
    {
        return ScheduledWarrantyCyclePolicy.ClassifyScheduledWarrantyCycle(item.MeasuredValue());
    }
}
