namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Payrollops.TaxForm.Proposal;

/// <summary>Functions of pending deduction cycle from notice.</summary>
public static class PendingDeductionCycleFromNotice
{
    public static Dictionary<string, double> SummarizeTaxFormNoticesForCycle(List<TaxFormNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<PendingDeductionCycle> DerivePendingDeductionCyclesFromNotice(List<TaxFormNotice> items)
    {
        var outValue = new List<PendingDeductionCycle>();
        foreach (var item in items)
        {
            var derived = PendingDeductionCycle.NewPendingDeductionCycle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePendingDeductionCycle(PendingDeductionCycle item)
    {
        return TaxFormNoticePolicy.ClassifyTaxFormNotice(item.EffectiveValue());
    }
}
