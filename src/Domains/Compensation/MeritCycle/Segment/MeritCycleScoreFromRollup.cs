namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Functions of merit cycle score from rollup.</summary>
public static class MeritCycleScoreFromRollup
{
    public static Dictionary<string, double> SummarizeSeasonalMeritCycleRollupsForScore(List<SeasonalMeritCycleRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<MeritCycleScore> DeriveMeritCycleScoresFromRollup(List<SeasonalMeritCycleRollup> items)
    {
        var outValue = new List<MeritCycleScore>();
        foreach (var item in items)
        {
            var derived = MeritCycleScore.NewMeritCycleScore(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeMeritCycleScore(MeritCycleScore item)
    {
        return SeasonalMeritCycleRollupPolicy.NormalizeSeasonalMeritCycleRollupCode(item.Code);
    }
}
