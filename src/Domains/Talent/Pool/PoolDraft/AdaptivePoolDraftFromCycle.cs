namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Functions of adaptive pool draft from cycle.</summary>
public static class AdaptivePoolDraftFromCycle
{
    public static Dictionary<string, double> SummarizeRollingCareerPathCyclesForDraft(List<RollingCareerPathCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<AdaptivePoolDraft> DeriveAdaptivePoolDraftsFromCycle(List<RollingCareerPathCycle> items)
    {
        var outValue = new List<AdaptivePoolDraft>();
        foreach (var item in items)
        {
            var derived = AdaptivePoolDraft.NewAdaptivePoolDraft(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreAdaptivePoolDraft(AdaptivePoolDraft item)
    {
        return StrictCareerPathProposalPolicy.NormalizeStrictCareerPathProposalCode(item.Code);
    }
}
