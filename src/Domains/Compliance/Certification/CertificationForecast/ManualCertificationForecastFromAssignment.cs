namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Compliance.PolicyAck.Assignment;

/// <summary>Functions of manual certification forecast from assignment.</summary>
public static class ManualCertificationForecastFromAssignment
{
    public static Dictionary<string, double> SummarizePolicyAckAssignmentsForForecast(List<PolicyAckAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<ManualCertificationForecast> DeriveManualCertificationForecastsFromAssignment(List<PolicyAckAssignment> items)
    {
        var outValue = new List<ManualCertificationForecast>();
        foreach (var item in items)
        {
            var derived = ManualCertificationForecast.NewManualCertificationForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeManualCertificationForecast(ManualCertificationForecast item)
    {
        return PolicyAckScorePolicy.PolicyAckScoreWeight(1, true);
    }
}
