namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Compensation.EquityGrant.GrantRecord;

/// <summary>Functions of tiered pay band draft from record.</summary>
public static class TieredPayBandDraftFromRecord
{
    public static Dictionary<string, double> SummarizeTieredEquityGrantRecordsForDraft(List<TieredEquityGrantRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<TieredPayBandDraft> DeriveTieredPayBandDraftsFromRecord(List<TieredEquityGrantRecord> items)
    {
        var outValue = new List<TieredPayBandDraft>();
        foreach (var item in items)
        {
            var derived = TieredPayBandDraft.NewTieredPayBandDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveTieredPayBandDraft(TieredPayBandDraft item)
    {
        return ScheduledEquityGrantVariancePolicy.ClassifyScheduledEquityGrantVariance(item.NetVolume());
    }
}
