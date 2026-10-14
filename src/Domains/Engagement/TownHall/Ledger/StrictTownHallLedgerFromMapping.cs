namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>Functions of strict town hall ledger from mapping.</summary>
public static class StrictTownHallLedgerFromMapping
{
    public static Dictionary<string, double> SummarizeTownHallMappingsForLedger(List<TownHallMapping> items)
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

    public static List<StrictTownHallLedger> DeriveStrictTownHallLedgersFromMapping(List<TownHallMapping> items)
    {
        var outValue = new List<StrictTownHallLedger>();
        foreach (var item in items)
        {
            var derived = StrictTownHallLedger.NewStrictTownHallLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeStrictTownHallLedger(StrictTownHallLedger item)
    {
        return TownHallOverridePolicy.NormalizeTownHallOverrideCode(item.Code);
    }
}
