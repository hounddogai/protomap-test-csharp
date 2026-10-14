namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Functions of capped retro pay preference from rollup.</summary>
public static class CappedRetroPayPreferenceFromRollup
{
    public static Dictionary<string, double> SummarizePayslipRollupsForPreference(List<PayslipRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<CappedRetroPayPreference> DeriveCappedRetroPayPreferencesFromRollup(List<PayslipRollup> items)
    {
        var outValue = new List<CappedRetroPayPreference>();
        foreach (var item in items)
        {
            var derived = CappedRetroPayPreference.NewCappedRetroPayPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyCappedRetroPayPreference(CappedRetroPayPreference item)
    {
        return WeightedPayslipDraftPolicy.ClassifyWeightedPayslipDraft(item.MeasuredValue());
    }
}
