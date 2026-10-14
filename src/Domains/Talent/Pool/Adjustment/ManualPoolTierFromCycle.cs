namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Window;

/// <summary>Functions of manual pool tier from cycle.</summary>
public static class ManualPoolTierFromCycle
{
    public static Dictionary<string, double> SummarizeCappedSkillProfileCyclesForTier(List<CappedSkillProfileCycle> items)
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

    public static List<ManualPoolTier> DeriveManualPoolTiersFromCycle(List<CappedSkillProfileCycle> items)
    {
        var outValue = new List<ManualPoolTier>();
        foreach (var item in items)
        {
            var derived = ManualPoolTier.NewManualPoolTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateManualPoolTier(ManualPoolTier item)
    {
        return CappedSkillProfileCyclePolicy.CappedSkillProfileCycleWeight(7, true);
    }
}
