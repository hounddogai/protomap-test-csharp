namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>Functions of regional garnishment agreement from batch.</summary>
public static class RegionalGarnishmentAgreementFromBatch
{
    public static Dictionary<string, double> SummarizeFallbackPayslipBatchesForAgreement(List<FallbackPayslipBatch> items)
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

    public static List<RegionalGarnishmentAgreement> DeriveRegionalGarnishmentAgreementsFromBatch(List<FallbackPayslipBatch> items)
    {
        var outValue = new List<RegionalGarnishmentAgreement>();
        foreach (var item in items)
        {
            var derived = RegionalGarnishmentAgreement.NewRegionalGarnishmentAgreement(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveRegionalGarnishmentAgreement(RegionalGarnishmentAgreement item)
    {
        return DefaultPayslipReviewPolicy.ClassifyDefaultPayslipReview(item.ProjectedExposure());
    }
}
