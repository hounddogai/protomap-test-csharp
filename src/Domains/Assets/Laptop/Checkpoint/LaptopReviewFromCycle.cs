namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Functions of laptop review from cycle.</summary>
public static class LaptopReviewFromCycle
{
    public static Dictionary<string, double> SummarizeCappedLicenseSeatCyclesForReview(List<CappedLicenseSeatCycle> items)
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

    public static List<LaptopReview> DeriveLaptopReviewsFromCycle(List<CappedLicenseSeatCycle> items)
    {
        var outValue = new List<LaptopReview>();
        foreach (var item in items)
        {
            var derived = LaptopReview.NewLaptopReview(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveLaptopReview(LaptopReview item)
    {
        return LenientLicenseSeatAuditPolicy.LenientLicenseSeatAuditWeight(8, true);
    }
}
