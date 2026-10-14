namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Talent.SkillProfile.Notice;

/// <summary>Functions of nomination exception from notice.</summary>
public static class NominationExceptionFromNotice
{
    public static Dictionary<string, double> SummarizeSkillProfileNoticesForException(List<SkillProfileNotice> items)
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

    public static List<NominationException> DeriveNominationExceptionsFromNotice(List<SkillProfileNotice> items)
    {
        var outValue = new List<NominationException>();
        foreach (var item in items)
        {
            var derived = NominationException.NewNominationException(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleNominationException(NominationException item)
    {
        return PrimarySkillProfileThresholdPolicy.PrimarySkillProfileThresholdWeight(6, true);
    }
}
