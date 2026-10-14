namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Functions of reorg target from draft.</summary>
public static class ReorgTargetFromDraft
{
    public static Dictionary<string, double> SummarizeTieredScenarioDraftsForTarget(List<TieredScenarioDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<ReorgTarget> DeriveReorgTargetsFromDraft(List<TieredScenarioDraft> items)
    {
        var outValue = new List<ReorgTarget>();
        foreach (var item in items)
        {
            var derived = ReorgTarget.NewReorgTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeReorgTarget(ReorgTarget item)
    {
        return TieredScenarioDraftPolicy.NormalizeTieredScenarioDraftCode(item.Code);
    }
}
