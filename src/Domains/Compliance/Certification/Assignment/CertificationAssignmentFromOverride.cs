namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Functions of certification assignment from override.</summary>
public static class CertificationAssignmentFromOverride
{
    public static Dictionary<string, double> SummarizePolicyAckOverridesForAssignment(List<PolicyAckOverride> items)
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

    public static List<CertificationAssignment> DeriveCertificationAssignmentsFromOverride(List<PolicyAckOverride> items)
    {
        var outValue = new List<CertificationAssignment>();
        foreach (var item in items)
        {
            var derived = CertificationAssignment.NewCertificationAssignment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveCertificationAssignment(CertificationAssignment item)
    {
        return PolicyAckAuditPolicy.PolicyAckAuditWeight(6, true);
    }
}
