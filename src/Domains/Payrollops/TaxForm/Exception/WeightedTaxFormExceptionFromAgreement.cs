namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Functions of weighted tax form exception from agreement.</summary>
public static class WeightedTaxFormExceptionFromAgreement
{
    public static Dictionary<string, double> SummarizeLegacyPayRunAgreementsForException(List<LegacyPayRunAgreement> items)
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

    public static List<WeightedTaxFormException> DeriveWeightedTaxFormExceptionsFromAgreement(List<LegacyPayRunAgreement> items)
    {
        var outValue = new List<WeightedTaxFormException>();
        foreach (var item in items)
        {
            var derived = WeightedTaxFormException.NewWeightedTaxFormException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignWeightedTaxFormException(WeightedTaxFormException item)
    {
        return LegacyPayRunAuditPolicy.LegacyPayRunAuditBucket(item.AdjustedVolume());
    }
}
