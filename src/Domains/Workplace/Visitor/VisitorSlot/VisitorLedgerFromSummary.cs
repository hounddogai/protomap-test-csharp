namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Functions of visitor ledger from summary.</summary>
public static class VisitorLedgerFromSummary
{
    public static Dictionary<string, double> SummarizeOccupancySummariesForLedger(List<OccupancySummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<VisitorLedger> DeriveVisitorLedgersFromSummary(List<OccupancySummary> items)
    {
        var outValue = new List<VisitorLedger>();
        foreach (var item in items)
        {
            var derived = VisitorLedger.NewVisitorLedger(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchVisitorLedger(VisitorLedger item)
    {
        return OccupancySummaryPolicy.NormalizeOccupancySummaryCode(item.Code);
    }
}
