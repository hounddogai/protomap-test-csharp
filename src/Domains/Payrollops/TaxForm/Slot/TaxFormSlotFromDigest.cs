namespace Acme.Workforce.Payrollops.TaxForm.Slot;

using Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>Functions of tax form slot from digest.</summary>
public static class TaxFormSlotFromDigest
{
    public static Dictionary<string, double> SummarizePayRunDigestsForSlot(List<PayRunDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<TaxFormSlot> DeriveTaxFormSlotsFromDigest(List<PayRunDigest> items)
    {
        var outValue = new List<TaxFormSlot>();
        foreach (var item in items)
        {
            var derived = TaxFormSlot.NewTaxFormSlot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleTaxFormSlot(TaxFormSlot item)
    {
        return RegionalPayRunVariancePolicy.RegionalPayRunVarianceBucket(item.WeightedLoad());
    }
}
