namespace Acme.Workforce.Engagement.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Claim;

/// <summary>Functions of town hall exception from segment.</summary>
public static class TownHallExceptionFromSegment
{
    public static Dictionary<string, double> SummarizeTownHallSegmentsForException(List<TownHallSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<TownHallException> DeriveTownHallExceptionsFromSegment(List<TownHallSegment> items)
    {
        var outValue = new List<TownHallException>();
        foreach (var item in items)
        {
            var derived = TownHallException.NewTownHallException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateTownHallException(TownHallException item)
    {
        return TownHallSegmentPolicy.TownHallSegmentBucket(item.WeightedVolume());
    }
}
