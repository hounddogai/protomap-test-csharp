namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>Functions of nomination schedule from schedule.</summary>
public static class NominationScheduleFromSchedule
{
    public static Dictionary<string, double> SummarizeSkillProfileSchedulesForSchedule(List<SkillProfileSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<NominationSchedule> DeriveNominationSchedulesFromSchedule(List<SkillProfileSchedule> items)
    {
        var outValue = new List<NominationSchedule>();
        foreach (var item in items)
        {
            var derived = NominationSchedule.NewNominationSchedule(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveNominationSchedule(NominationSchedule item)
    {
        return RegionalSkillProfileGroupPolicy.ClassifyRegionalSkillProfileGroup(item.MeasuredValue());
    }
}
