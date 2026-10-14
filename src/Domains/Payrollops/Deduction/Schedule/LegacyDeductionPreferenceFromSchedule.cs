namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Functions of legacy deduction preference from schedule.</summary>
public static class LegacyDeductionPreferenceFromSchedule
{
    public static Dictionary<string, double> SummarizeGarnishmentSchedulesForPreference(List<GarnishmentSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<LegacyDeductionPreference> DeriveLegacyDeductionPreferencesFromSchedule(List<GarnishmentSchedule> items)
    {
        var outValue = new List<LegacyDeductionPreference>();
        foreach (var item in items)
        {
            var derived = LegacyDeductionPreference.NewLegacyDeductionPreference(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveLegacyDeductionPreference(LegacyDeductionPreference item)
    {
        return FallbackGarnishmentRecordPolicy.ClassifyFallbackGarnishmentRecord(item.WeightedVolume());
    }
}
