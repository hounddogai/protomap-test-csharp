namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Orgdesign.Headcount.Draft;

/// <summary>Functions of global position rollup from target.</summary>
public static class GlobalPositionRollupFromTarget
{
    public static Dictionary<string, double> SummarizeDefaultHeadcountTargetsForRollup(List<DefaultHeadcountTarget> items)
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

    public static List<GlobalPositionRollup> DeriveGlobalPositionRollupsFromTarget(List<DefaultHeadcountTarget> items)
    {
        var outValue = new List<GlobalPositionRollup>();
        foreach (var item in items)
        {
            var derived = GlobalPositionRollup.NewGlobalPositionRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeGlobalPositionRollup(GlobalPositionRollup item)
    {
        return DefaultHeadcountTargetPolicy.NormalizeDefaultHeadcountTargetCode(item.Code);
    }
}
