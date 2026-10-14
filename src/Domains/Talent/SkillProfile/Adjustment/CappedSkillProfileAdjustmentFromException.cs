namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Talent.Pool.Agreement;

/// <summary>Functions of capped skill profile adjustment from exception.</summary>
public static class CappedSkillProfileAdjustmentFromException
{
    public static Dictionary<string, double> SummarizeRollingPoolExceptionsForAdjustment(List<RollingPoolException> items)
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

    public static List<CappedSkillProfileAdjustment> DeriveCappedSkillProfileAdjustmentsFromException(List<RollingPoolException> items)
    {
        var outValue = new List<CappedSkillProfileAdjustment>();
        foreach (var item in items)
        {
            var derived = CappedSkillProfileAdjustment.NewCappedSkillProfileAdjustment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeCappedSkillProfileAdjustment(CappedSkillProfileAdjustment item)
    {
        return RollingPoolExceptionPolicy.ClassifyRollingPoolException(item.NetExposure());
    }
}
