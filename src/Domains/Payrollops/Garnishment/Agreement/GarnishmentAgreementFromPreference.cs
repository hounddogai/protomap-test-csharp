namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>Functions of garnishment agreement from preference.</summary>
public static class GarnishmentAgreementFromPreference
{
    public static Dictionary<string, double> SummarizeDeductionPreferencesForAgreement(List<DeductionPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<GarnishmentAgreement> DeriveGarnishmentAgreementsFromPreference(List<DeductionPreference> items)
    {
        var outValue = new List<GarnishmentAgreement>();
        foreach (var item in items)
        {
            var derived = GarnishmentAgreement.NewGarnishmentAgreement(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveGarnishmentAgreement(GarnishmentAgreement item)
    {
        return AdaptiveDeductionEntryPolicy.NormalizeAdaptiveDeductionEntryCode(item.Code);
    }
}
