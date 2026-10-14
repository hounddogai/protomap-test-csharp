namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Talent.Mobility.MobilityAudit;

/// <summary>Functions of default nomination assignment from audit.</summary>
public static class DefaultNominationAssignmentFromAudit
{
    public static Dictionary<string, double> SummarizePrimaryMobilityAuditsForAssignment(List<PrimaryMobilityAudit> items)
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

    public static List<DefaultNominationAssignment> DeriveDefaultNominationAssignmentsFromAudit(List<PrimaryMobilityAudit> items)
    {
        var outValue = new List<DefaultNominationAssignment>();
        foreach (var item in items)
        {
            var derived = DefaultNominationAssignment.NewDefaultNominationAssignment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleDefaultNominationAssignment(DefaultNominationAssignment item)
    {
        return LegacyMobilityOverridePolicy.LegacyMobilityOverrideWeight(9, true);
    }
}
