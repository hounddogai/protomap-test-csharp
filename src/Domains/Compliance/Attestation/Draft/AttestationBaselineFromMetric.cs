namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Functions of attestation baseline from metric.</summary>
public static class AttestationBaselineFromMetric
{
    public static Dictionary<string, double> SummarizePolicyAckMetricsForBaseline(List<PolicyAckMetric> items)
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

    public static List<AttestationBaseline> DeriveAttestationBaselinesFromMetric(List<PolicyAckMetric> items)
    {
        var outValue = new List<AttestationBaseline>();
        foreach (var item in items)
        {
            var derived = AttestationBaseline.NewAttestationBaseline(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAttestationBaseline(AttestationBaseline item)
    {
        return PolicyAckMetricPolicy.NormalizePolicyAckMetricCode(item.Code);
    }
}
