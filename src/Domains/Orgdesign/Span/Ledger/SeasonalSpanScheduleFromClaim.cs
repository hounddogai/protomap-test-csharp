namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Functions of seasonal span schedule from claim.</summary>
public static class SeasonalSpanScheduleFromClaim
{
    public static Dictionary<string, double> SummarizeCostModelClaimsForSchedule(List<CostModelClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<SeasonalSpanSchedule> DeriveSeasonalSpanSchedulesFromClaim(List<CostModelClaim> items)
    {
        var outValue = new List<SeasonalSpanSchedule>();
        foreach (var item in items)
        {
            var derived = SeasonalSpanSchedule.NewSeasonalSpanSchedule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSeasonalSpanSchedule(SeasonalSpanSchedule item)
    {
        return CostModelEntryPolicy.ClassifyCostModelEntry(item.MeasuredValue());
    }
}
