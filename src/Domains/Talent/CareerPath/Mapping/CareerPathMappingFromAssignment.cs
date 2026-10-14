namespace Acme.Workforce.Talent.CareerPath.Mapping;

using Acme.Workforce.Talent.Nomination.Assignment;

/// <summary>Functions of career path mapping from assignment.</summary>
public static class CareerPathMappingFromAssignment
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

    public static List<CareerPathMapping> DeriveCareerPathMappingsFromAssignment(List<DefaultNominationAssignment> items)
    {
        var outValue = new List<CareerPathMapping>();
        foreach (var item in items)
        {
            var derived = CareerPathMapping.NewCareerPathMapping(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchCareerPathMapping(CareerPathMapping item)
    {
        return DefaultNominationAssignmentPolicy.NormalizeDefaultNominationAssignmentCode(item.Code);
    }
}
