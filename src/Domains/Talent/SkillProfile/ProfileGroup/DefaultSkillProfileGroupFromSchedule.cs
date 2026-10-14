namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Functions of default skill profile group from schedule.</summary>
public static class DefaultSkillProfileGroupFromSchedule
{
    public static Dictionary<string, double> SummarizeStrictPoolSchedulesForGroup(List<StrictPoolSchedule> items)
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

    public static List<DefaultSkillProfileGroup> DeriveDefaultSkillProfileGroupsFromSchedule(List<StrictPoolSchedule> items)
    {
        var outValue = new List<DefaultSkillProfileGroup>();
        foreach (var item in items)
        {
            var derived = DefaultSkillProfileGroup.NewDefaultSkillProfileGroup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectDefaultSkillProfileGroup(DefaultSkillProfileGroup item)
    {
        return StrictPoolSchedulePolicy.StrictPoolScheduleBucket(item.MeasuredValue());
    }
}
