namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Functions of balance record from rule.</summary>
public static class BalanceRecordFromRule
{
    public static Dictionary<string, double> SummarizeHolidayRulesForRecord(List<HolidayRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<BalanceRecord> DeriveBalanceRecordsFromRule(List<HolidayRule> items)
    {
        var outValue = new List<BalanceRecord>();
        foreach (var item in items)
        {
            var derived = BalanceRecord.NewBalanceRecord(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeBalanceRecord(BalanceRecord item)
    {
        return HolidayVariancePolicy.HolidayVarianceWeight(6, true);
    }
}
