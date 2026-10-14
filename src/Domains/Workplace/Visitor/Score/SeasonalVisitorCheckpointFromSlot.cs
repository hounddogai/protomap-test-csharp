namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Functions of seasonal visitor checkpoint from slot.</summary>
public static class SeasonalVisitorCheckpointFromSlot
{
    public static Dictionary<string, double> SummarizeFallbackDeskSlotsForCheckpoint(List<FallbackDeskSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<SeasonalVisitorCheckpoint> DeriveSeasonalVisitorCheckpointsFromSlot(List<FallbackDeskSlot> items)
    {
        var outValue = new List<SeasonalVisitorCheckpoint>();
        foreach (var item in items)
        {
            var derived = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankSeasonalVisitorCheckpoint(SeasonalVisitorCheckpoint item)
    {
        return DeskExceptionPolicy.NormalizeDeskExceptionCode(item.Code);
    }
}
