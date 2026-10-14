namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Functions of skill profile request from assignment.</summary>
public static class SkillProfileRequestFromAssignment
{
    public static Dictionary<string, double> SummarizeMobilityAssignmentsForRequest(List<MobilityAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<SkillProfileRequest> DeriveSkillProfileRequestsFromAssignment(List<MobilityAssignment> items)
    {
        var outValue = new List<SkillProfileRequest>();
        foreach (var item in items)
        {
            var derived = SkillProfileRequest.NewSkillProfileRequest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveSkillProfileRequest(SkillProfileRequest item)
    {
        return WeightedMobilityRecordPolicy.WeightedMobilityRecordWeight(2, true);
    }
}
