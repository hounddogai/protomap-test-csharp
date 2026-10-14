namespace Acme.Workforce.Orgdesign.Reorg.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Functions of reorg rule from rule.</summary>
public static class ReorgRuleFromRule
{
    public static Dictionary<string, double> SummarizePositionRulesForRule(List<PositionRule> items)
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

    public static List<ReorgRule> DeriveReorgRulesFromRule(List<PositionRule> items)
    {
        var outValue = new List<ReorgRule>();
        foreach (var item in items)
        {
            var derived = ReorgRule.NewReorgRule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectReorgRule(ReorgRule item)
    {
        return TieredPositionBundlePolicy.NormalizeTieredPositionBundleCode(item.Code);
    }
}
