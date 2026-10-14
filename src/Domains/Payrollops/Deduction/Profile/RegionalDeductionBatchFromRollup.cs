namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Functions of regional deduction batch from rollup.</summary>
public static class RegionalDeductionBatchFromRollup
{
    public static Dictionary<string, double> SummarizeLenientPayslipRollupsForBatch(List<LenientPayslipRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<RegionalDeductionBatch> DeriveRegionalDeductionBatchesFromRollup(List<LenientPayslipRollup> items)
    {
        var outValue = new List<RegionalDeductionBatch>();
        foreach (var item in items)
        {
            var derived = RegionalDeductionBatch.NewRegionalDeductionBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankRegionalDeductionBatch(RegionalDeductionBatch item)
    {
        return FallbackPayslipDigestPolicy.FallbackPayslipDigestWeight(6, true);
    }
}
