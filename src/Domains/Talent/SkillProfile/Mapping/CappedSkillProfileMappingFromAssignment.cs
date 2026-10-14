namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Functions of capped skill profile mapping from assignment.</summary>
public static class CappedSkillProfileMappingFromAssignment
{
    public static Dictionary<string, double> SummarizeDefaultNominationAssignmentsForMapping(List<DefaultNominationAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<CappedSkillProfileMapping> DeriveCappedSkillProfileMappingsFromAssignment(List<DefaultNominationAssignment> items)
    {
        var outValue = new List<CappedSkillProfileMapping>();
        foreach (var item in items)
        {
            var derived = CappedSkillProfileMapping.NewCappedSkillProfileMapping(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceCappedSkillProfileMapping(CappedSkillProfileMapping item)
    {
        return DefaultNominationAssignmentPolicy.NormalizeDefaultNominationAssignmentCode(item.Code);
    }
}
