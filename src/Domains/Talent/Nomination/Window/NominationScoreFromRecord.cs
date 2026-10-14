namespace Acme.Workforce.Talent.Nomination.Window;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Functions of nomination score from record.</summary>
public static class NominationScoreFromRecord
{
    public static Dictionary<string, double> SummarizeLenientSuccessionPlanRecordsForScore(List<LenientSuccessionPlanRecord> items)
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

    public static List<NominationScore> DeriveNominationScoresFromRecord(List<LenientSuccessionPlanRecord> items)
    {
        var outValue = new List<NominationScore>();
        foreach (var item in items)
        {
            var derived = NominationScore.NewNominationScore(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeNominationScore(NominationScore item)
    {
        return SuccessionPlanRollupPolicy.SuccessionPlanRollupBucket(item.MeasuredValue());
    }
}
