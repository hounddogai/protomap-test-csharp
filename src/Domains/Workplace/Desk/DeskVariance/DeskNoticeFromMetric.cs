namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Functions of desk notice from metric.</summary>
public static class DeskNoticeFromMetric
{
    public static Dictionary<string, double> SummarizeTieredDeskMetricsForNotice(List<TieredDeskMetric> items)
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

    public static List<DeskNotice> DeriveDeskNoticesFromMetric(List<TieredDeskMetric> items)
    {
        var outValue = new List<DeskNotice>();
        foreach (var item in items)
        {
            var derived = DeskNotice.NewDeskNotice(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyDeskNotice(DeskNotice item)
    {
        return PendingDeskAllocationPolicy.PendingDeskAllocationWeight(7, true);
    }
}
