namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Functions of seasonal pay run batch from profile.</summary>
public static class SeasonalPayRunBatchFromProfile
{
    public static Dictionary<string, double> SummarizeTaxFormProfilesForBatch(List<TaxFormProfile> items)
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

    public static List<SeasonalPayRunBatch> DeriveSeasonalPayRunBatchesFromProfile(List<TaxFormProfile> items)
    {
        var outValue = new List<SeasonalPayRunBatch>();
        foreach (var item in items)
        {
            var derived = SeasonalPayRunBatch.NewSeasonalPayRunBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceSeasonalPayRunBatch(SeasonalPayRunBatch item)
    {
        return LenientTaxFormNoticePolicy.ClassifyLenientTaxFormNotice(item.ProjectedValue());
    }
}
