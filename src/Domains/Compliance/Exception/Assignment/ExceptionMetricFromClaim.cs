namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Functions of exception metric from claim.</summary>
public static class ExceptionMetricFromClaim
{
    public static Dictionary<string, double> SummarizeDefaultAttestationClaimsForMetric(List<DefaultAttestationClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<ExceptionMetric> DeriveExceptionMetricsFromClaim(List<DefaultAttestationClaim> items)
    {
        var outValue = new List<ExceptionMetric>();
        foreach (var item in items)
        {
            var derived = ExceptionMetric.NewExceptionMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeExceptionMetric(ExceptionMetric item)
    {
        return AttestationAllocationPolicy.AttestationAllocationWeight(1, true);
    }
}
