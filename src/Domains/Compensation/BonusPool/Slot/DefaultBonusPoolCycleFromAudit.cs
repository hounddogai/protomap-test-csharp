namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Functions of default bonus pool cycle from audit.</summary>
public static class DefaultBonusPoolCycleFromAudit
{
    public static Dictionary<string, double> SummarizeBonusPoolAuditsForCycle(List<BonusPoolAudit> items)
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

    public static List<DefaultBonusPoolCycle> DeriveDefaultBonusPoolCyclesFromAudit(List<BonusPoolAudit> items)
    {
        var outValue = new List<DefaultBonusPoolCycle>();
        foreach (var item in items)
        {
            var derived = DefaultBonusPoolCycle.NewDefaultBonusPoolCycle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyDefaultBonusPoolCycle(DefaultBonusPoolCycle item)
    {
        return BonusPoolPolicyPolicy.ClassifyBonusPoolPolicy(item.MeasuredValue());
    }
}
