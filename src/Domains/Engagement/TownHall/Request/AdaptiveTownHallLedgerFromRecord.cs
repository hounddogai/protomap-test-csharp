namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>Functions of adaptive town hall ledger from record.</summary>
public static class AdaptiveTownHallLedgerFromRecord
{
    public static Dictionary<string, double> SummarizeGlobalPulseSurveyRecordsForLedger(List<GlobalPulseSurveyRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<AdaptiveTownHallLedger> DeriveAdaptiveTownHallLedgersFromRecord(List<GlobalPulseSurveyRecord> items)
    {
        var outValue = new List<AdaptiveTownHallLedger>();
        foreach (var item in items)
        {
            var derived = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAdaptiveTownHallLedger(AdaptiveTownHallLedger item)
    {
        return GlobalPulseSurveyRecordPolicy.GlobalPulseSurveyRecordBucket(item.MeasuredValue());
    }
}
