namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Functions of legacy pay band window from ledger.</summary>
public static class LegacyPayBandWindowFromLedger
{
    public static Dictionary<string, double> SummarizeTieredEquityGrantLedgersForWindow(List<TieredEquityGrantLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<LegacyPayBandWindow> DeriveLegacyPayBandWindowsFromLedger(List<TieredEquityGrantLedger> items)
    {
        var outValue = new List<LegacyPayBandWindow>();
        foreach (var item in items)
        {
            var derived = LegacyPayBandWindow.NewLegacyPayBandWindow(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLegacyPayBandWindow(LegacyPayBandWindow item)
    {
        return AdaptiveEquityGrantOverridePolicy.NormalizeAdaptiveEquityGrantOverrideCode(item.Code);
    }
}
