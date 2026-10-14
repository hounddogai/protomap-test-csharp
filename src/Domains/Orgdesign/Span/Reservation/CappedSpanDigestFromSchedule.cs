namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Schedule;

/// <summary>Functions of capped span digest from schedule.</summary>
public static class CappedSpanDigestFromSchedule
{
    public static Dictionary<string, double> SummarizeManualSpanSchedulesForDigest(List<ManualSpanSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<CappedSpanDigest> DeriveCappedSpanDigestsFromSchedule(List<ManualSpanSchedule> items)
    {
        var outValue = new List<CappedSpanDigest>();
        foreach (var item in items)
        {
            var derived = CappedSpanDigest.NewCappedSpanDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankCappedSpanDigest(CappedSpanDigest item)
    {
        return ManualSpanSchedulePolicy.ClassifyManualSpanSchedule(item.EffectiveValue());
    }
}
