namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Functions of cost model draft from template.</summary>
public static class CostModelDraftFromTemplate
{
    public static Dictionary<string, double> SummarizeHeadcountTemplatesForDraft(List<HeadcountTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<CostModelDraft> DeriveCostModelDraftsFromTemplate(List<HeadcountTemplate> items)
    {
        var outValue = new List<CostModelDraft>();
        foreach (var item in items)
        {
            var derived = CostModelDraft.NewCostModelDraft(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCostModelDraft(CostModelDraft item)
    {
        return WeightedHeadcountSchedulePolicy.ClassifyWeightedHeadcountSchedule(item.ProjectedTotal());
    }
}
