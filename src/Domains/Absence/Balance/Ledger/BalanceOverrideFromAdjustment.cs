namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Functions of balance override from adjustment.</summary>
public static class BalanceOverrideFromAdjustment
{
    public static Dictionary<string, double> SummarizeLenientHolidayAdjustmentsForOverride(List<LenientHolidayAdjustment> items)
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

    public static List<BalanceOverride> DeriveBalanceOverridesFromAdjustment(List<LenientHolidayAdjustment> items)
    {
        var outValue = new List<BalanceOverride>();
        foreach (var item in items)
        {
            var derived = BalanceOverride.NewBalanceOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyBalanceOverrideExtra(BalanceOverride item)
    {
        return LenientHolidayAdjustmentPolicy.NormalizeLenientHolidayAdjustmentCode(item.Code);
    }
}
