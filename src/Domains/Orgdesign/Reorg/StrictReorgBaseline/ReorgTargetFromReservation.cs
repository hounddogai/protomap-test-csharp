namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Functions of reorg target from reservation.</summary>
public static class ReorgTargetFromReservation
{
    public static Dictionary<string, double> SummarizeHeadcountReservationsForTarget(List<HeadcountReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<ReorgTarget> DeriveReorgTargetsFromReservation(List<HeadcountReservation> items)
    {
        var outValue = new List<ReorgTarget>();
        foreach (var item in items)
        {
            var derived = ReorgTarget.NewReorgTarget(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceReorgTarget(ReorgTarget item)
    {
        return HeadcountSummaryPolicy.HeadcountSummaryWeight(6, true);
    }
}
