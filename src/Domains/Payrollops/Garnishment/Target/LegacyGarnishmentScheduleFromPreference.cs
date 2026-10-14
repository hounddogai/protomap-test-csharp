namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Functions of legacy garnishment schedule from preference.</summary>
public static class LegacyGarnishmentScheduleFromPreference
{
    public static Dictionary<string, double> SummarizeLegacyDeductionPreferencesForSchedule(List<LegacyDeductionPreference> items)
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

    public static List<LegacyGarnishmentSchedule> DeriveLegacyGarnishmentSchedulesFromPreference(List<LegacyDeductionPreference> items)
    {
        var outValue = new List<LegacyGarnishmentSchedule>();
        foreach (var item in items)
        {
            var derived = LegacyGarnishmentSchedule.NewLegacyGarnishmentSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreLegacyGarnishmentSchedule(LegacyGarnishmentSchedule item)
    {
        return DeductionReviewPolicy.NormalizeDeductionReviewCode(item.Code);
    }
}
