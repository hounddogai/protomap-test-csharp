namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Functions of primary career path digest from schedule.</summary>
public static class PrimaryCareerPathDigestFromSchedule
{
    public static Dictionary<string, double> SummarizeStrictSkillProfileSchedulesForDigest(List<StrictSkillProfileSchedule> items)
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

    public static List<PrimaryCareerPathDigest> DerivePrimaryCareerPathDigestsFromSchedule(List<StrictSkillProfileSchedule> items)
    {
        var outValue = new List<PrimaryCareerPathDigest>();
        foreach (var item in items)
        {
            var derived = PrimaryCareerPathDigest.NewPrimaryCareerPathDigest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePrimaryCareerPathDigest(PrimaryCareerPathDigest item)
    {
        return DefaultSkillProfileSchedulePolicy.ClassifyDefaultSkillProfileSchedule(item.ProjectedExposure());
    }
}
