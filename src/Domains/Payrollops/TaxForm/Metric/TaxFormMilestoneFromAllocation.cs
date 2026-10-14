namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Functions of tax form milestone from allocation.</summary>
public static class TaxFormMilestoneFromAllocation
{
    public static Dictionary<string, double> SummarizeGarnishmentAllocationsForMilestone(List<GarnishmentAllocation> items)
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

    public static List<TaxFormMilestone> DeriveTaxFormMilestonesFromAllocation(List<GarnishmentAllocation> items)
    {
        var outValue = new List<TaxFormMilestone>();
        foreach (var item in items)
        {
            var derived = TaxFormMilestone.NewTaxFormMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreTaxFormMilestone(TaxFormMilestone item)
    {
        return GarnishmentExceptionPolicy.GarnishmentExceptionBucket(item.WeightedValue());
    }
}
