namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Functions of merit cycle draft from segment.</summary>
public static class MeritCycleDraftFromSegment
{
    public static Dictionary<string, double> SummarizeBonusPoolSegmentsForDraft(List<BonusPoolSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<MeritCycleDraft> DeriveMeritCycleDraftsFromSegment(List<BonusPoolSegment> items)
    {
        var outValue = new List<MeritCycleDraft>();
        foreach (var item in items)
        {
            var derived = MeritCycleDraft.NewMeritCycleDraft(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreMeritCycleDraft(MeritCycleDraft item)
    {
        return BonusPoolCheckpointPolicy.BonusPoolCheckpointWeight(8, true);
    }
}
