namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Functions of rolling garnishment variance from adjustment.</summary>
public static class RollingGarnishmentVarianceFromAdjustment
{
    public static Dictionary<string, double> SummarizeRollingDeductionAdjustmentsForVariance(List<RollingDeductionAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<RollingGarnishmentVariance> DeriveRollingGarnishmentVariancesFromAdjustment(List<RollingDeductionAdjustment> items)
    {
        var outValue = new List<RollingGarnishmentVariance>();
        foreach (var item in items)
        {
            var derived = RollingGarnishmentVariance.NewRollingGarnishmentVariance(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignRollingGarnishmentVariance(RollingGarnishmentVariance item)
    {
        return DeductionAllocationPolicy.DeductionAllocationBucket(item.MeasuredValue());
    }
}
