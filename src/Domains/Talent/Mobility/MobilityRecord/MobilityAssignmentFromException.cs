namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Functions of mobility assignment from exception.</summary>
public static class MobilityAssignmentFromException
{
    public static Dictionary<string, double> SummarizeDefaultCareerPathExceptionsForAssignment(List<DefaultCareerPathException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<MobilityAssignment> DeriveMobilityAssignmentsFromException(List<DefaultCareerPathException> items)
    {
        var outValue = new List<MobilityAssignment>();
        foreach (var item in items)
        {
            var derived = MobilityAssignment.NewMobilityAssignment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchMobilityAssignment(MobilityAssignment item)
    {
        return CappedCareerPathBudgetPolicy.CappedCareerPathBudgetWeight(8, true);
    }
}
