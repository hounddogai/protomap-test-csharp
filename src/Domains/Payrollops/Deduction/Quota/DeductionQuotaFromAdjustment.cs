namespace Acme.Workforce.Payrollops.Deduction.Quota;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Functions of deduction quota from adjustment.</summary>
public static class DeductionQuotaFromAdjustment
{
    public static Dictionary<string, double> SummarizeTaxFormAdjustmentsForQuota(List<TaxFormAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<DeductionQuota> DeriveDeductionQuotasFromAdjustment(List<TaxFormAdjustment> items)
    {
        var outValue = new List<DeductionQuota>();
        foreach (var item in items)
        {
            var derived = DeductionQuota.NewDeductionQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeDeductionQuota(DeductionQuota item)
    {
        return TaxFormAgreementPolicy.ClassifyTaxFormAgreement(item.AdjustedValue());
    }
}
